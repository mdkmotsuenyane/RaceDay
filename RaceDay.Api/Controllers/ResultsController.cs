using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.DTOs;
using RaceDay.Models;
using System.Security.Claims;

namespace RaceDay.Controllers
{
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET /api/v1/entries/{entryId}/result
        // ============================================================
        [HttpGet("api/v1/entries/{entryId:int}/result")]
        [Authorize]
        public async Task<IActionResult> GetByEntry(int entryId)
        {
            var entry = await _context.Entries
                .Include(e => e.Participant)
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e => e.EntryID == entryId);

            if (entry == null) return NotFound(new { message = "Entry not found." });

            if (!await CanAccessEntry(entry))
                return Forbid();

            var result = await _context.Results
                .FirstOrDefaultAsync(r => r.EntryID == entryId);

            if (result == null)
                return NotFound(new { message = "No result yet for this entry." });

            return Ok(ToDto(result));
        }

        // ============================================================
        // GET /api/v1/events/{eventId}/results
        // ============================================================
        [HttpGet("api/v1/events/{eventId:int}/results")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByEvent(int eventId)
        {
            var eventExists = await _context.Events.AnyAsync(e => e.EventID == eventId);
            if (!eventExists) return NotFound(new { message = "Event not found." });

            var leaderboard = await _context.Results
                .Where(r => r.Entry != null && r.Entry.EventId == eventId)
                .Include(r => r.Entry)
                    .ThenInclude(e => e!.Participant)
                .Include(r => r.Entry)
                    .ThenInclude(e => e!.Category)
                .OrderBy(r => r.Position ?? int.MaxValue)
                .ThenBy(r => r.FinishTime)
                .Select(r => new LeaderboardItemDto
                {
                    ResultID = r.ResultID,
                    EntryID = r.EntryID,
                    ParticipantID = r.Entry != null ? r.Entry.ParticipantID : null,
                    ParticipantName = r.Entry != null && r.Entry.Participant != null
                                        ? ((r.Entry.Participant.FirstName ?? "") + " " +
                                           (r.Entry.Participant.LastName ?? "")).Trim()
                                        : null,
                    CategoryName = r.Entry != null && r.Entry.Category != null
                                        ? r.Entry.Category.Name
                                        : null,
                    Position = r.Position,
                    FinishTime = r.FinishTime,
                    ChipTime = r.ChipTime,
                    GunTime = r.GunTime,
                    Status = r.Status
                })
                .ToListAsync();

            return Ok(leaderboard);
        }

        // ============================================================
        // GET /api/v1/participants/{participantId}/results
        // FIXED: Now includes Event Name, Date, Category, and Total Finishers
        // ============================================================
        [HttpGet("api/v1/participants/{participantId:int}/results")]
        [Authorize]
        public async Task<IActionResult> GetByParticipant(int participantId)
        {
            var participant = await _context.Participants.FirstOrDefaultAsync(p => p.ParticipantID == participantId);
            if (participant == null) return NotFound();

            var currentUserId = GetCurrentUserId();
            var isSelf = currentUserId.HasValue && participant.UserID == currentUserId.Value;
            if (!isSelf && !User.IsInRole("Organiser")) return Forbid();

            var results = await _context.Results
                .Where(r => r.Entry != null && r.Entry.ParticipantID == participantId)
                .Include(r => r.Entry)
                    .ThenInclude(e => e!.Event)
                .Include(r => r.Entry)
                    .ThenInclude(e => e!.Category)
                .Select(r => new ResultResponseDto
                {
                    ResultID = r.ResultID,
                    EntryID = r.EntryID,
                    FinishTime = r.FinishTime,
                    Position = r.Position,
                    Status = r.Status,
                    ChipTime = r.ChipTime,
                    GunTime = r.GunTime,

                    // New fields for the UI
                    EventName = r.Entry!.Event!.Name,
                    EventDate = r.Entry.Event.EventDate,
                    CategoryName = r.Entry.Category != null ? r.Entry.Category.Name : "Open",

                    // Calculate total finishers for this specific event
                    TotalFinishers = _context.Results.Count(res =>
                        res.Entry!.EventId == r.Entry.EventId &&
                        res.Status == "Finished")
                })
                .ToListAsync();

            return Ok(results);
        }

        // ============================================================
        // POST /api/v1/entries/{entryId}/result
        // ============================================================
        [HttpPost("api/v1/entries/{entryId:int}/result")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Create(int entryId, [FromBody] CreateResultRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var entry = await _context.Entries
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e => e.EntryID == entryId);

            if (entry == null) return NotFound(new { message = "Entry not found." });

            var organiserId = await GetCurrentOrganiserID();
            if (organiserId == null
                || entry.Event == null
                || entry.Event.OrganiserID != organiserId.Value)
                return Forbid();

            var exists = await _context.Results.AnyAsync(r => r.EntryID == entryId);
            if (exists)
                return Conflict(new { message = "A result already exists for this entry." });

            var result = new Result
            {
                EntryID = entryId,
                FinishTime = dto.FinishTime,
                Position = dto.Position,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Finished" : dto.Status,
                ChipTime = dto.ChipTime,
                GunTime = dto.GunTime
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById),
                new { id = result.ResultID },
                ToDto(result));
        }

        // ============================================================
        // PUT /api/v1/results/{id}
        // ============================================================
        [HttpPut("api/v1/results/{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateResultRequestDto dto)
        {
            var result = await _context.Results
                .Include(r => r.Entry)
                    .ThenInclude(e => e!.Event)
                .FirstOrDefaultAsync(r => r.ResultID == id);

            if (result == null) return NotFound();

            var organiserId = await GetCurrentOrganiserID();
            if (organiserId == null
                || result.Entry == null
                || result.Entry.Event == null
                || result.Entry.Event.OrganiserID != organiserId.Value)
                return Forbid();

            if (dto.FinishTime != null) result.FinishTime = dto.FinishTime;
            if (dto.Position.HasValue) result.Position = dto.Position;
            if (dto.Status != null) result.Status = dto.Status;
            if (dto.ChipTime != null) result.ChipTime = dto.ChipTime;
            if (dto.GunTime != null) result.GunTime = dto.GunTime;

            await _context.SaveChangesAsync();

            return Ok(ToDto(result));
        }

        // ============================================================
        // DELETE /api/v1/results/{id}
        // ============================================================
        [HttpDelete("api/v1/results/{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _context.Results
                .Include(r => r.Entry)
                    .ThenInclude(e => e!.Event)
                .FirstOrDefaultAsync(r => r.ResultID == id);

            if (result == null) return NotFound();

            var organiserId = await GetCurrentOrganiserID();
            if (organiserId == null
                || result.Entry == null
                || result.Entry.Event == null
                || result.Entry.Event.OrganiserID != organiserId.Value)
                return Forbid();

            _context.Results.Remove(result);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ============================================================
        // GET /api/v1/results/{id}
        // ============================================================
        [HttpGet("api/v1/results/{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _context.Results
                .Include(r => r.Entry)
                    .ThenInclude(e => e!.Participant)
                .Include(r => r.Entry)
                    .ThenInclude(e => e!.Event)
                .FirstOrDefaultAsync(r => r.ResultID == id);

            if (result == null) return NotFound();

            if (result.Entry == null)
                return Ok(ToDto(result));

            if (!await CanAccessEntry(result.Entry))
                return Forbid();

            return Ok(ToDto(result));
        }

        // ---------- helpers ----------

        private static ResultResponseDto ToDto(Result r) => new ResultResponseDto
        {
            ResultID = r.ResultID,
            EntryID = r.EntryID,
            FinishTime = r.FinishTime,
            Position = r.Position,
            Status = r.Status,
            ChipTime = r.ChipTime,
            GunTime = r.GunTime
        };

        private int? GetCurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("sub")?.Value;
            return int.TryParse(idClaim, out var id) ? id : null;
        }

        private async Task<int?> GetCurrentOrganiserID()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return null;

            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserID == userId.Value);
            return organiser?.OrganiserID;
        }

        private async Task<bool> CanAccessEntry(Entry entry)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null) return false;

            if (entry.Participant != null && entry.Participant.UserID == currentUserId.Value)
                return true;

            if (User.IsInRole("Organiser") && entry.Event != null)
            {
                var organiserId = await GetCurrentOrganiserID();
                if (organiserId != null && entry.Event.OrganiserID == organiserId.Value)
                    return true;
            }

            return false;
        }
    }
}

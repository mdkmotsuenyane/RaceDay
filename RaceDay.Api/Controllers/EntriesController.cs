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
    public class EntriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EntriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET /api/v1/participants/{participantId}/entries
        // ============================================================
        [HttpGet("api/v1/participants/{participantId:int}/entries")]
        [Authorize]
        public async Task<IActionResult> GetByParticipant(int participantId)
        {
            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.ParticipantID == participantId);

            if (participant == null) return NotFound(new { message = "Participant not found." });

            var currentUserId = GetCurrentUserId();
            var isSelf = currentUserId.HasValue && participant.UserID == currentUserId.Value;
            var isOrganiser = User.IsInRole("Organiser");

            if (!isSelf && !isOrganiser) return Forbid();

            var entries = await _context.Entries
                .Where(e => e.ParticipantID == participantId)
                .Select(e => ToDto(e))
                .ToListAsync();

            return Ok(entries);
        }

        // ============================================================
        // GET /api/v1/events/{eventId}/entries
        // ============================================================
        [HttpGet("api/v1/events/{eventId:int}/entries")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetByEvent(int eventId)
        {
            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null) return NotFound(new { message = "Event not found." });

            var organiserId = await GetCurrentOrganiserID();
            if (organiserId == null || ev.OrganiserID != organiserId.Value)
                return Forbid();

            var entries = await _context.Entries
                .Where(e => e.EventId == eventId)
                .Include(e => e.Participant)
                    .ThenInclude(p => p!.User)
                .Select(e => new EntryWithParticipantDto
                {
                    EntryID = e.EntryID,
                    ParticipantID = e.ParticipantID,
                    EventID = e.EventId,
                    CategoryID = e.CategoryID,
                    RegistrationDate = e.RegistrationDate,
                    Status = e.Status,
                    StartNumber = e.StartNumber,
                    Paid = e.Paid,
                    ParticipantFirstName = e.Participant != null ? e.Participant.FirstName : null,
                    ParticipantLastName = e.Participant != null ? e.Participant.LastName : null,
                    ParticipantEmail = e.Participant != null && e.Participant.User != null
                                             ? e.Participant.User.Email
                                             : null,

                    // NEW: Map the profile picture URL
                    ParticipantProfileImageUrl = e.Participant != null && e.Participant.User != null
                                             ? e.Participant.User.ProfileImageUrl
                                             : null
                })
                .ToListAsync();

            return Ok(entries);
        }

        // ============================================================
        // GET /api/v1/entries/{id}
        // ============================================================
        [HttpGet("api/v1/entries/{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var entry = await _context.Entries
                .Include(e => e.Participant)
                .FirstOrDefaultAsync(e => e.EntryID == id);

            if (entry == null) return NotFound();

            if (!await CanAccessEntry(entry))
                return Forbid();

            return Ok(ToDto(entry));
        }

        // ============================================================
        // POST /api/v1/entries
        // ============================================================
        [HttpPost("api/v1/entries")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> Create([FromBody] CreateEntryRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.ParticipantID == dto.ParticipantID);
            if (participant == null)
                return BadRequest(new { message = "Participant not found." });

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue || participant.UserID != currentUserId.Value)
                return Forbid();

            var ev = await _context.Events.FindAsync(dto.EventID);
            if (ev == null)
                return BadRequest(new { message = "Event not found." });
            if (ev.Status != "Open")
                return BadRequest(new { message = "This event is not open for entries." });

            if (dto.CategoryID.HasValue)
            {
                var categoryOk = await _context.Categories
                    .AnyAsync(c => c.CategoryID == dto.CategoryID.Value && c.EventID == dto.EventID);
                if (!categoryOk)
                    return BadRequest(new { message = "Category does not belong to this event." });
            }

            var already = await _context.Entries
                .AnyAsync(e => e.ParticipantID == dto.ParticipantID && e.EventId == dto.EventID);
            if (already)
                return BadRequest(new { message = "Participant is already entered in this event." });

            var entry = new Entry
            {
                ParticipantID = dto.ParticipantID,
                EventId = dto.EventID,
                CategoryID = dto.CategoryID,
                RegistrationDate = DateTime.UtcNow,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Pending" : dto.Status,
                Paid = dto.Paid ?? 0
            };

            _context.Entries.Add(entry);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = entry.EntryID }, ToDto(entry));
        }

        // ============================================================
        // PUT /api/v1/entries/{id}
        // ============================================================
        [HttpPut("api/v1/entries/{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEntryRequestDto dto)
        {
            var entry = await _context.Entries
                .Include(e => e.Event)
                .Include(e => e.Participant)
                .FirstOrDefaultAsync(e => e.EntryID == id);

            if (entry == null) return NotFound();

            if (!await CanAccessEntry(entry))
                return Forbid();

            var isOrganiser = User.IsInRole("Organiser");
            var isParticipant = User.IsInRole("Participant");

            if (isOrganiser)
            {
                if (dto.Status != null) entry.Status = dto.Status;
                if (dto.StartNumber.HasValue) entry.StartNumber = dto.StartNumber;
                if (dto.Paid.HasValue) entry.Paid = dto.Paid;
            }
            else if (isParticipant)
            {
                if (dto.Paid.HasValue) entry.Paid = dto.Paid;
            }
            else
            {
                return Forbid();
            }

            await _context.SaveChangesAsync();

            return Ok(ToDto(entry));
        }

        // ============================================================
        // DELETE /api/v1/entries/{id}
        // ============================================================
        [HttpDelete("api/v1/entries/{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var entry = await _context.Entries
                .Include(e => e.Participant)
                .FirstOrDefaultAsync(e => e.EntryID == id);

            if (entry == null) return NotFound();

            if (!await CanAccessEntry(entry))
                return Forbid();

            var hasResult = await _context.Results.AnyAsync(r => r.EntryID == id);
            if (hasResult)
                return Conflict(new { message = "Cannot withdraw — a result exists for this entry." });

            if (entry.EventId.HasValue)
            {
                var ev = await _context.Events.FindAsync(entry.EventId.Value);
                if (ev != null && ev.EventDate.Date < DateTime.Today)
                    return Conflict(new { message = "Cannot withdraw — the race has already started." });
            }

            _context.Entries.Remove(entry);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ---------- helpers ----------

        private static EntryResponseDto ToDto(Entry e) => new EntryResponseDto
        {
            EntryID = e.EntryID,
            ParticipantID = e.ParticipantID,
            EventID = e.EventId,
            CategoryID = e.CategoryID,
            RegistrationDate = e.RegistrationDate,
            Status = e.Status,
            StartNumber = e.StartNumber,
            Paid = e.Paid
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

            if (User.IsInRole("Organiser") && entry.EventId.HasValue)
            {
                var organiserId = await GetCurrentOrganiserID();
                if (organiserId != null)
                {
                    var ev = await _context.Events.FindAsync(entry.EventId.Value);
                    if (ev != null && ev.OrganiserID == organiserId.Value)
                        return true;
                }
            }

            return false;
        }
    }
}

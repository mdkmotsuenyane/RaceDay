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
    [Route("api/v1/participants")]
    public class ParticipantsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ParticipantsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET /api/v1/participants
        // List all participants. Role: Organiser / Admin
        // 200 OK
        // ============================================================
        [HttpGet]
        [Authorize(Roles = "Organiser,Admin")]
        public async Task<IActionResult> GetAll()
        {
            var participants = await _context.Participants
                .Select(p => new ParticipantResponseDto
                {
                    ParticipantID = p.ParticipantID,
                    UserID = p.UserID,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    DateOfBirth = p.DateOfBirth,
                    Gender = p.Gender,
                    EmergencyContact = p.EmergencyContact
                })
                .ToListAsync();

            return Ok(participants);
        }

        // ============================================================
        // GET /api/v1/participants/{id}
        // Get a specific participant. Role: Self or Organiser
        // 200 OK | 403 Forbidden | 404 Not Found
        // ============================================================
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var participant = await _context.Participants.FindAsync(id);
            if (participant == null) return NotFound();

            var currentUserId = GetCurrentUserId();
            var isSelf = currentUserId.HasValue && participant.UserID == currentUserId.Value;
            var isOrganiser = User.IsInRole("Organiser");

            if (!isSelf && !isOrganiser) return Forbid();

            return Ok(new ParticipantResponseDto
            {
                ParticipantID = participant.ParticipantID,
                UserID = participant.UserID,
                FirstName = participant.FirstName,
                LastName = participant.LastName,
                DateOfBirth = participant.DateOfBirth,
                Gender = participant.Gender,
                EmergencyContact = participant.EmergencyContact
            });
        }

        // ============================================================
        // POST /api/v1/participants
        // Create a participant profile for an existing User. Public.
        // 201 Created | 400 Bad Request
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateParticipantRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userExists = await _context.User.AnyAsync(u => u.UserId == dto.UserID);
            if (!userExists)
                return BadRequest(new { message = "User does not exist." });

            var alreadyParticipant = await _context.Participants
                .AnyAsync(p => p.UserID == dto.UserID);
            if (alreadyParticipant)
                return BadRequest(new { message = "UserID already has a participant profile." });

            var participant = new Participant
            {
                UserID = dto.UserID,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                EmergencyContact = dto.EmergencyContact
            };

            _context.Participants.Add(participant);
            await _context.SaveChangesAsync();

            var response = new ParticipantResponseDto
            {
                ParticipantID = participant.ParticipantID,
                UserID = participant.UserID,
                FirstName = participant.FirstName,
                LastName = participant.LastName,
                DateOfBirth = participant.DateOfBirth,
                Gender = participant.Gender,
                EmergencyContact = participant.EmergencyContact
            };

            return CreatedAtAction(nameof(GetById), new { id = participant.ParticipantID }, response);
        }

        // ============================================================
        // PUT /api/v1/participants/{id}
        // Update participant (Self or Organiser).
        // 200 OK | 403 Forbidden | 404 Not Found
        // ============================================================
        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateParticipantRequestDto dto)
        {
            var participant = await _context.Participants.FindAsync(id);
            if (participant == null) return NotFound();

            var currentUserId = GetCurrentUserId();
            var isSelf = currentUserId.HasValue && participant.UserID == currentUserId.Value;
            var isOrganiser = User.IsInRole("Organiser");

            if (!isSelf && !isOrganiser) return Forbid();

            if (dto.FirstName != null) participant.FirstName = dto.FirstName;
            if (dto.LastName != null) participant.LastName = dto.LastName;
            if (dto.EmergencyContact != null) participant.EmergencyContact = dto.EmergencyContact;

            await _context.SaveChangesAsync();

            return Ok(new ParticipantResponseDto
            {
                ParticipantID = participant.ParticipantID,
                UserID = participant.UserID,
                FirstName = participant.FirstName,
                LastName = participant.LastName,
                DateOfBirth = participant.DateOfBirth,
                Gender = participant.Gender,
                EmergencyContact = participant.EmergencyContact
            });
        }

        // ============================================================
        // DELETE /api/v1/participants/{id}
        // Delete participant (checks for active entries). Admin only.
        // 204 No Content | 404 Not Found | 409 Conflict
        // ============================================================
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var participant = await _context.Participants.FindAsync(id);
            if (participant == null) return NotFound();

            var hasEntries = await _context.Entries.AnyAsync(e => e.ParticipantID == id);
            if (hasEntries)
                return Conflict(new { message = "Cannot delete participant with existing entries." });

            _context.Participants.Remove(participant);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ---------- helper ----------

        private int? GetCurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("sub")?.Value;
            return int.TryParse(idClaim, out var id) ? id : null;
        }
    }
}
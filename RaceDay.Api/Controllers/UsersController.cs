using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.DTOs;
using System.Security.Claims;

namespace RaceDay.Controllers
{
    [ApiController]
    [Route("api/v1/users")]
    public class UsersController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public UsersController(RaceDayDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET /api/v1/users/me
        // Current user's profile with role-specific fields.
        // Authenticated (any role). 200 OK | 401 | 404
        // ============================================================
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMe()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.User
                .Include(u => u.Participant)
                .Include(u => u.Organiser)
                .FirstOrDefaultAsync(u => u.UserId == userId.Value);

            if (user == null) return NotFound();

            return Ok(BuildProfile(user));
        }

        // ============================================================
        // PUT /api/v1/users/me
        // Update the current user's own profile.
        // Authenticated (any role). 200 OK | 400 | 404
        // ============================================================
        [HttpPut("me")]
        [Authorize]
        public async Task<IActionResult> UpdateMe([FromBody] UpdateUserProfileRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.User
                .Include(u => u.Participant)
                .Include(u => u.Organiser)
                .FirstOrDefaultAsync(u => u.UserId == userId.Value);

            if (user == null) return NotFound();

            // Update common fields
            if (dto.FullName != null) user.FullName = dto.FullName;
            if (dto.Phone != null) user.Phone = dto.Phone;

            // emergencyContact belongs to Participant only
            if (dto.EmergencyContact != null && user.Participant != null)
                user.Participant.EmergencyContact = dto.EmergencyContact;

            await _context.SaveChangesAsync();

            return Ok(BuildProfile(user));
        }

        // ---------- helpers ----------

        private static UserProfileResponseDto BuildProfile(Models.User user)
        {
            var dto = new UserProfileResponseDto
            {
                UserId = user.UserId,
                Email = user.Email,
                Role = user.Role,
                FullName = user.FullName,
                Phone = user.Phone,
                CreatedAt = user.CreatedAt
            };

            if (user.Participant != null)
            {
                dto.ParticipantID = user.Participant.ParticipantID;
                dto.FirstName = user.Participant.FirstName;
                dto.LastName = user.Participant.LastName;
                dto.DateOfBirth = user.Participant.DateOfBirth;
                dto.Gender = user.Participant.Gender;
                dto.EmergencyContact = user.Participant.EmergencyContact;
            }

            if (user.Organiser != null)
            {
                dto.OrganiserID = user.Organiser.OrganiserID;
                dto.OrganiserName = user.Organiser.Name;
                dto.Address = user.Organiser.Address;
            }

            return dto;
        }

        private int? GetCurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("sub")?.Value;
            return int.TryParse(idClaim, out var id) ? id : null;
        }
    }
}
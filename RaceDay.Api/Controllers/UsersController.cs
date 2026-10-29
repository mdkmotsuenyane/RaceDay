using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Api.Services;
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

        [HttpPut("me")]
        [Authorize]
        public async Task<IActionResult> UpdateMe([FromBody] UpdateUserProfileRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.User
                .Include(u => u.Participant)
                .FirstOrDefaultAsync(u => u.UserId == userId.Value);

            if (user == null) return NotFound();

            if (dto.FullName != null) user.FullName = dto.FullName;
            if (dto.Phone != null) user.Phone = dto.Phone;

            if (dto.EmergencyContact != null && user.Participant != null)
                user.Participant.EmergencyContact = dto.EmergencyContact;

            await _context.SaveChangesAsync();

            return Ok(BuildProfile(user));
        }

        [HttpPost("me/profile-picture")]
        [Authorize]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file, [FromServices] BlobService blobService)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.User.FirstOrDefaultAsync(u => u.UserId == userId.Value);
            if (user == null) return NotFound();

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            try
            {
                using var stream = file.OpenReadStream();
                var imageUrl = await blobService.UploadAsync(stream, file.FileName);

                user.ProfileImageUrl = imageUrl;
                await _context.SaveChangesAsync();

                return Ok(new { profileImageUrl = imageUrl });
            }
            catch (Exception ex)
            {
                // If Azure fails, this catches the error and returns it to the MVC app
                return StatusCode(500, $"Azure Upload Error: {ex.Message}");
            }
        }

        [HttpPost("me/profile-picture/delete")]
        [Authorize]
        public async Task<IActionResult> DeleteProfilePicture([FromServices] BlobService blobService)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.User.FirstOrDefaultAsync(u => u.UserId == userId.Value);
            if (user == null) return NotFound();

            if (!string.IsNullOrEmpty(user.ProfileImageUrl))
            {
                try
                {
                    await blobService.DeleteAsync(user.ProfileImageUrl);
                }
                catch { /* Ignore Azure delete errors */ }

                user.ProfileImageUrl = null;
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        private static UserProfileResponseDto BuildProfile(Models.User user)
        {
            var dto = new UserProfileResponseDto
            {
                UserId = user.UserId,
                Email = user.Email,
                Role = user.Role,
                FullName = user.FullName,
                Phone = user.Phone,
                ProfileImageUrl = user.ProfileImageUrl,
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

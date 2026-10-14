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
    [Route("api/v1/organisers")]
    public class OrganisersController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public OrganisersController(RaceDayDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET /api/v1/organisers
        // List all organisers. Role: Organiser / Admin
        // 200 OK
        // ============================================================
        [HttpGet]
        [Authorize(Roles = "Organiser,Admin")]
        public async Task<IActionResult> GetAll()
        {
            var organisers = await _context.Organisers
                .Select(o => new OrganiserResponseDto
                {
                    OrganiserID = o.OrganiserID,
                    UserID = o.UserID,
                    Name = o.Name,
                    Address = o.Address
                })
                .ToListAsync();

            return Ok(organisers);
        }

        // ============================================================
        // GET /api/v1/organisers/{id}
        // Get a specific organiser. Role: Organiser / Admin
        // 200 OK | 404 Not Found
        // ============================================================
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Organiser,Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            var organiser = await _context.Organisers.FindAsync(id);
            if (organiser == null) return NotFound();

            return Ok(new OrganiserResponseDto
            {
                OrganiserID = organiser.OrganiserID,
                UserID = organiser.UserID,
                Name = organiser.Name,
                Address = organiser.Address
            });
        }

        // ============================================================
        // POST /api/v1/organisers
        // Create an organiser profile for an existing User. Public.
        // 201 Created | 400 Bad Request (user missing / already organiser)
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrganiserRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userExists = await _context.User.AnyAsync(u => u.UserId == dto.UserID);
            if (!userExists)
                return BadRequest(new { message = "User does not exist." });

            var alreadyOrganiser = await _context.Organisers
                .AnyAsync(o => o.UserID == dto.UserID);
            if (alreadyOrganiser)
                return BadRequest(new { message = "UserID already has an organiser profile." });

            var organiser = new Organiser
            {
                UserID = dto.UserID,
                Name = dto.Name,
                Address = dto.Address
            };

            _context.Organisers.Add(organiser);
            await _context.SaveChangesAsync();

            var response = new OrganiserResponseDto
            {
                OrganiserID = organiser.OrganiserID,
                UserID = organiser.UserID,
                Name = organiser.Name,
                Address = organiser.Address
            };

            return CreatedAtAction(nameof(GetById), new { id = organiser.OrganiserID }, response);
        }

        // ============================================================
        // PUT /api/v1/organisers/{id}
        // Update organiser (Self). Role: Organiser
        // 200 OK | 403 Forbidden | 404 Not Found
        // ============================================================
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateOrganiserRequestDto dto)
        {
            var organiser = await _context.Organisers.FindAsync(id);
            if (organiser == null) return NotFound();

            var currentUserId = GetCurrentUserId();
            if (currentUserId == null || organiser.UserID != currentUserId.Value)
                return Forbid();

            if (dto.Name != null) organiser.Name = dto.Name;
            if (dto.Address != null) organiser.Address = dto.Address;

            await _context.SaveChangesAsync();

            return Ok(new OrganiserResponseDto
            {
                OrganiserID = organiser.OrganiserID,
                UserID = organiser.UserID,
                Name = organiser.Name,
                Address = organiser.Address
            });
        }

        // ============================================================
        // DELETE /api/v1/organisers/{id}
        // Delete organiser (checks for existing events). Admin only.
        // 204 No Content | 404 Not Found | 409 Conflict
        // ============================================================
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var organiser = await _context.Organisers.FindAsync(id);
            if (organiser == null) return NotFound();

            var hasEvents = await _context.Events.AnyAsync(e => e.OrganiserID == id);
            if (hasEvents)
                return Conflict(new { message = "Cannot delete organiser with existing events." });

            _context.Organisers.Remove(organiser);
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
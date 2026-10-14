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
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET /api/v1/events/{eventId}/categories
        // Get all categories for an event. Public.
        // 200 OK | 404 Not Found (event)
        // ============================================================
        [HttpGet("api/v1/events/{eventId:int}/categories")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByEvent(int eventId)
        {
            var eventExists = await _context.Events.AnyAsync(e => e.EventID == eventId);
            if (!eventExists) return NotFound(new { message = "Event not found." });

            var categories = await _context.Categories
                .Where(c => c.EventID == eventId)
                .Select(c => new CategoryResponseDto
                {
                    CategoryID = c.CategoryID,
                    EventID = c.EventID,
                    Name = c.Name,
                    Description = c.Description,
                    MinAge = c.MinAge,
                    MaxAge = c.MaxAge,
                    Gender = c.Gender,
                    Distance = c.Distance
                })
                .ToListAsync();

            return Ok(categories);
        }

        // ============================================================
        // GET /api/v1/categories/{id}
        // Get a single category. Public.
        // 200 OK | 404 Not Found
        // ============================================================
        [HttpGet("api/v1/categories/{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            return Ok(MapToDto(category));
        }

        // ============================================================
        // POST /api/v1/events/{eventId}/categories
        // Add a new category to an event. Organiser (Owner).
        // 201 Created | 400 Bad Request | 403 Forbidden | 404 Not Found
        // ============================================================
        [HttpPost("api/v1/events/{eventId:int}/categories")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Create(int eventId, [FromBody] CreateCategoryRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null) return NotFound(new { message = "Event not found." });

            var organiserId = await GetCurrentOrganiserID();
            if (organiserId == null || ev.OrganiserID != organiserId.Value)
                return Forbid();

            var category = new Category
            {
                EventID = eventId,
                Name = dto.Name,
                Description = dto.Description,
                MinAge = dto.MinAge,
                MaxAge = dto.MaxAge,
                Gender = dto.Gender,
                Distance = dto.Distance
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById),
                new { id = category.CategoryID },
                MapToDto(category));
        }

        // ============================================================
        // PUT /api/v1/categories/{id}
        // Update a category. Organiser (Owner).
        // 200 OK | 403 Forbidden | 404 Not Found
        // ============================================================
        [HttpPut("api/v1/categories/{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryRequestDto dto)
        {
            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null) return NotFound();

            var organiserId = await GetCurrentOrganiserID();
            if (organiserId == null
                || category.Event == null
                || category.Event.OrganiserID != organiserId.Value)
                return Forbid();

            if (dto.Name != null) category.Name = dto.Name;
            if (dto.Description != null) category.Description = dto.Description;
            if (dto.MinAge.HasValue) category.MinAge = dto.MinAge;
            if (dto.MaxAge.HasValue) category.MaxAge = dto.MaxAge;
            if (dto.Gender != null) category.Gender = dto.Gender;
            if (dto.Distance.HasValue) category.Distance = dto.Distance;

            await _context.SaveChangesAsync();

            return Ok(MapToDto(category));
        }

        // ============================================================
        // DELETE /api/v1/categories/{id}
        // Delete a category (checks for entries). Organiser (Owner).
        // 204 No Content | 403 Forbidden | 404 Not Found | 409 Conflict
        // ============================================================
        [HttpDelete("api/v1/categories/{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null) return NotFound();

            var organiserId = await GetCurrentOrganiserID();
            if (organiserId == null
                || category.Event == null
                || category.Event.OrganiserID != organiserId.Value)
                return Forbid();

            var hasEntries = await _context.Entries.AnyAsync(e => e.CategoryID == id);
            if (hasEntries)
                return Conflict(new { message = "Cannot delete category with existing entries." });

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ---------- helpers ----------

        private static CategoryResponseDto MapToDto(Category c) => new CategoryResponseDto
        {
            CategoryID = c.CategoryID,
            EventID = c.EventID,
            Name = c.Name,
            Description = c.Description,
            MinAge = c.MinAge,
            MaxAge = c.MaxAge,
            Gender = c.Gender,
            Distance = c.Distance
        };

        private async Task<int?> GetCurrentOrganiserID()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("sub")?.Value;
            if (!int.TryParse(idClaim, out var userId)) return null;

            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserID == userId);
            return organiser?.OrganiserID;
        }
    }
}
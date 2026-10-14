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
    [Route("api/v1/events")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET /api/v1/events?status=Open&location=Cape%20Town
        // List all events with optional filters. Public.
        // ============================================================
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] string? status,
                                                [FromQuery] string? location)
        {
            var query = _context.Events.AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(e => e.Status == status);

            if (!string.IsNullOrWhiteSpace(location))
                query = query.Where(e => e.Location != null &&
                                         e.Location.Contains(location));

            var events = await query
                .OrderBy(e => e.EventDate)
                .Select(e => ToResponseDto(e))
                .ToListAsync();

            return Ok(events);
        }

        // ============================================================
        // GET /api/v1/events/upcoming
        // Events with date >= today, sorted ascending. Public.
        // ============================================================
        [HttpGet("upcoming")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUpcoming()
        {
            var today = DateTime.Today;

            var events = await _context.Events
                .Where(e => e.EventDate >= today)
                .OrderBy(e => e.EventDate)
                .Select(e => ToResponseDto(e))
                .ToListAsync();

            return Ok(events);
        }

        // ============================================================
        // GET /api/v1/events/{id}
        // Full event details (organiser + categories + weather + route). Public.
        // ============================================================
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Organiser)
                .Include(e => e.Categories)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (ev == null) return NotFound();

            return Ok(ToDetailsDto(ev));
        }

        // ============================================================
        // GET /api/v1/events/{id}/weather
        // Convenience endpoint: only weather fields. Public.
        // ============================================================
        [HttpGet("{id:int}/weather")]
        [AllowAnonymous]
        public async Task<IActionResult> GetWeather(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            return Ok(new WeatherResponseDto
            {
                EventID = ev.EventID,
                WeatherForecast = ev.WeatherForecast,
                Temperature = ev.Temperature,
                WindSpeed = ev.WindSpeed,
                Precipitation = ev.Precipitation
            });
        }

        // ============================================================
        // POST /api/v1/events
        // Create a new event. Organiser (must have a profile).
        // ============================================================
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Create([FromBody] CreateEventRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var organiserId = await GetCurrentOrganiserID();
            if (organiserId == null)
                return Forbid();

            var ev = new Event
            {
                OrganiserID = organiserId.Value,
                Name = dto.Name,
                Description = dto.Description,
                EventDate = dto.EventDate,
                StartTime = dto.StartTime,
                Location = dto.Location,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Open" : dto.Status,
                StartPoint = dto.StartPoint,
                FinishPoint = dto.FinishPoint
            };

            _context.Events.Add(ev);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById),
                new { id = ev.EventID },
                ToResponseDto(ev));
        }

        // ============================================================
        // PUT /api/v1/events/{id}
        // Update an existing event. Organiser (Owner).
        // ============================================================
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEventRequestDto dto)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            var organiserId = await GetCurrentOrganiserID();
            if (organiserId == null || ev.OrganiserID != organiserId.Value)
                return Forbid();

            if (dto.Name != null) ev.Name = dto.Name;
            if (dto.Description != null) ev.Description = dto.Description;
            if (dto.EventDate.HasValue) ev.EventDate = dto.EventDate.Value;
            if (dto.StartTime != null) ev.StartTime = dto.StartTime;
            if (dto.Location != null) ev.Location = dto.Location;
            if (dto.Status != null) ev.Status = dto.Status;
            if (dto.StartPoint != null) ev.StartPoint = dto.StartPoint;
            if (dto.FinishPoint != null) ev.FinishPoint = dto.FinishPoint;
            if (dto.MapUrl != null) ev.MapUrl = dto.MapUrl;
            if (dto.ElevationUrl != null) ev.ElevationUrl = dto.ElevationUrl;
            if (dto.WeatherForecast != null) ev.WeatherForecast = dto.WeatherForecast;
            if (dto.Temperature.HasValue) ev.Temperature = dto.Temperature;
            if (dto.WindSpeed.HasValue) ev.WindSpeed = dto.WindSpeed;
            if (dto.Precipitation.HasValue) ev.Precipitation = dto.Precipitation;

            await _context.SaveChangesAsync();

            return Ok(ToResponseDto(ev));
        }

        // ============================================================
        // DELETE /api/v1/events/{id}
        // Delete an event. Organiser (Owner).
        // 204 | 403 | 404 | 409 (has entries)
        // ============================================================
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            var organiserId = await GetCurrentOrganiserID();
            if (organiserId == null || ev.OrganiserID != organiserId.Value)
                return Forbid();

            var hasEntries = await _context.Entries.AnyAsync(e => e.EventId == id);
            if (hasEntries)
                return Conflict(new { message = "Cannot delete event with existing entries." });

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ---------- helpers ----------

        private static EventResponseDto ToResponseDto(Event e) => new EventResponseDto
        {
            EventID = e.EventID,
            OrganiserID = e.OrganiserID,
            Name = e.Name,
            Description = e.Description,
            EventDate = e.EventDate,
            StartTime = e.StartTime,
            Location = e.Location,
            Status = e.Status
        };

        private static EventDetailsDto ToDetailsDto(Event e) => new EventDetailsDto
        {
            EventID = e.EventID,
            OrganiserID = e.OrganiserID,
            OrganiserName = e.Organiser != null ? e.Organiser.Name : null,
            Name = e.Name,
            Description = e.Description,
            EventDate = e.EventDate,
            StartTime = e.StartTime,
            Location = e.Location,
            Status = e.Status,
            StartPoint = e.StartPoint,
            FinishPoint = e.FinishPoint,
            MapUrl = e.MapUrl,
            ElevationUrl = e.ElevationUrl,
            WeatherForecast = e.WeatherForecast,
            Temperature = e.Temperature,
            WindSpeed = e.WindSpeed,
            Precipitation = e.Precipitation,
            Categories = e.Categories.Select(c => new CategoryResponseDto
            {
                CategoryID = c.CategoryID,
                EventID = c.EventID,
                Name = c.Name,
                Description = c.Description,
                MinAge = c.MinAge,
                MaxAge = c.MaxAge,
                Gender = c.Gender,
                Distance = c.Distance
            }).ToList()
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
    }
}
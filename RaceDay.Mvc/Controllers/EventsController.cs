using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;
using RaceDay.DTOs;

namespace RaceDay.Mvc.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApiClient _api;

        public EventsController(ApiClient api)
        {
            _api = api;
        }

        // GET: /Events
        public async Task<IActionResult> Index(string searchString)
        {
            // Fetch all events from the API
            var events = await _api.GetAsync<List<EventResponseDto>>("/api/v1/events");

            if (events == null)
                return View(new List<EventResponseDto>());

            // Filtering logic based on search string
            if (!string.IsNullOrEmpty(searchString))
            {
                events = events.Where(e =>
                    e.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    (e.Location != null && e.Location.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            // Pass the current search string back to the view so the search box stays populated
            ViewBag.CurrentFilter = searchString;

            return View(events);
        }

        // GET: /Events/Details/5
        public async Task<IActionResult> Details(int id)
        {
            // Fetch full details for a single event (includes categories, weather, etc.)
            var ev = await _api.GetAsync<EventDetailsDto>($"/api/v1/events/{id}");

            if (ev == null)
            {
                return NotFound();
            }

            return View(ev);
        }
    }
}

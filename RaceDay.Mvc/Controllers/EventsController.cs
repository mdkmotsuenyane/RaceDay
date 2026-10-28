using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;
using RaceDay.DTOs; // Reusing API DTOs

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
        public async Task<IActionResult> Index()
        {
            // Fetch all events from the API
            var events = await _api.GetAsync<List<EventResponseDto>>("/api/v1/events");

            // If the API is down, events might be null
            return View(events ?? new List<EventResponseDto>());
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

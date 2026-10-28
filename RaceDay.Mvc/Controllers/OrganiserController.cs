using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;
using RaceDay.DTOs;

namespace RaceDay.Mvc.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly ApiClient _api;

        public OrganiserController(ApiClient api)
        {
            _api = api;
        }

        // GET: /Organiser/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Account");

            // Get my profile to find my OrganiserID
            var profile = await _api.GetAsync<UserProfileResponseDto>("/api/v1/users/me");

            // Get all events (in a real app, the API would filter by organiserId, 
            // but we'll filter here for simplicity)
            var events = await _api.GetAsync<List<EventResponseDto>>("/api/v1/events");

            if (events != null && profile != null)
            {
                // Only show events that belong to this organiser
                events = events.Where(e => e.OrganiserID == profile.OrganiserID).ToList();
            }

            return View(events ?? new List<EventResponseDto>());
        }

        // GET: /Organiser/CreateEvent
        public IActionResult CreateEvent()
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Account");

            return View();
        }

        // POST: /Organiser/CreateEvent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(CreateEventRequestDto model)
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid) return View(model);

            var response = await _api.PostAsync("/api/v1/events", model);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Event created successfully!";
                return RedirectToAction(nameof(Dashboard));
            }

            ModelState.AddModelError("", "Failed to create event. Please check your data.");
            return View(model);
        }
    }
}

using System.Collections.Generic;
using System.Linq;
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

            var profile = await _api.GetAsync<UserProfileResponseDto>("/api/v1/users/me");
            var events = await _api.GetAsync<List<EventResponseDto>>("/api/v1/events");

            if (events != null && profile != null)
            {
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

        // GET: /Organiser/EditEvent/5
        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Account");

            var ev = await _api.GetAsync<EventDetailsDto>($"/api/v1/events/{id}");
            if (ev == null) return NotFound();

            // Map the details DTO to the update DTO for the form
            var model = new UpdateEventRequestDto
            {
                Name = ev.Name,
                Description = ev.Description,
                EventDate = ev.EventDate,
                StartTime = ev.StartTime,
                Location = ev.Location,
                Status = ev.Status,
                StartPoint = ev.StartPoint,
                FinishPoint = ev.FinishPoint
            };

            ViewBag.EventId = id;
            return View(model);
        }

        // POST: /Organiser/EditEvent/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(int id, UpdateEventRequestDto model)
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                ViewBag.EventId = id;
                return View(model);
            }

            var response = await _api.PutAsync($"/api/v1/events/{id}", model);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Event updated successfully!";
                return RedirectToAction(nameof(Dashboard));
            }

            ModelState.AddModelError("", "Failed to update event.");
            ViewBag.EventId = id;
            return View(model);
        }

        // POST: /Organiser/DeleteEvent/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Account");

            var response = await _api.DeleteAsync($"/api/v1/events/{id}");

            if (response.IsSuccessStatusCode)
                TempData["Success"] = "Event deleted successfully.";
            else
                TempData["Error"] = "Failed to delete event. It might have entries.";

            return RedirectToAction(nameof(Dashboard));
        }
    }
}

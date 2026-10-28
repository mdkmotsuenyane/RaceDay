using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models; // Need this for CreateEventViewModel
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

            return View(new CreateEventViewModel());
        }

        // POST: /Organiser/CreateEvent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(CreateEventViewModel model)
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid) return View(model);

            // 1. Map ViewModel to API DTO and create the event
            var dto = new CreateEventRequestDto
            {
                Name = model.Name,
                Description = model.Description,
                EventDate = model.EventDate,
                StartTime = model.StartTime,
                Location = model.Location,
                Status = "Open",
                StartPoint = model.StartPoint,
                FinishPoint = model.FinishPoint
            };

            var response = await _api.PostAsync("/api/v1/events", dto);

            if (response.IsSuccessStatusCode)
            {
                // 2. If successful, get the new EventID
                var createdEvent = await response.Content.ReadFromJsonAsync<EventResponseDto>();

                // 3. If an image was uploaded, send it to the Blob Storage API
                if (model.ImageFile != null && model.ImageFile.Length > 0 && createdEvent != null)
                {
                    using var content = new MultipartFormDataContent();
                    using var stream = model.ImageFile.OpenReadStream();

                    content.Add(new StreamContent(stream), "file", model.ImageFile.FileName);

                    // Call the new API endpoint we created
                    await _api.PostFileAsync($"/api/v1/events/{createdEvent.EventID}/upload-image", content);
                }

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

        // ==========================================
        // NEW METHODS FOR CATEGORIES
        // ==========================================

        // GET: /Organiser/ManageCategories/5
        public async Task<IActionResult> ManageCategories(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Account");

            var ev = await _api.GetAsync<EventDetailsDto>($"/api/v1/events/{id}");
            if (ev == null) return NotFound();

            var categories = await _api.GetAsync<List<CategoryResponseDto>>($"/api/v1/events/{id}/categories");

            ViewBag.EventId = id;
            ViewBag.EventName = ev.Name;

            return View(categories ?? new List<CategoryResponseDto>());
        }

        // POST: /Organiser/AddCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(int eventId, CreateCategoryRequestDto model)
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill in all required fields correctly.";
                return RedirectToAction(nameof(ManageCategories), new { id = eventId });
            }

            var response = await _api.PostAsync($"/api/v1/events/{eventId}/categories", model);

            if (response.IsSuccessStatusCode)
                TempData["Success"] = "Category added successfully!";
            else
                TempData["Error"] = "Failed to add category.";

            return RedirectToAction(nameof(ManageCategories), new { id = eventId });
        }
    }
}

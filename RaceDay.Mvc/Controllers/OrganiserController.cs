using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
using RaceDay.DTOs;
using System.Net.Http; // Required for MultipartFormDataContent
using System.Net.Http.Json; // Required for ReadFromJsonAsync
using System.IO; // Required for OpenReadStream

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

                // GAP 2: Get entry counts for each event
                var entryCounts = new Dictionary<int, int>();
                foreach (var ev in events)
                {
                    var entries = await _api.GetAsync<List<EntryWithParticipantDto>>($"/api/v1/events/{ev.EventID}/entries");
                    entryCounts[ev.EventID] = entries?.Count ?? 0;
                }
                ViewBag.EntryCounts = entryCounts;
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
                var createdEvent = await response.Content.ReadFromJsonAsync<EventResponseDto>();

                if (model.ImageFile != null && model.ImageFile.Length > 0 && createdEvent != null)
                {
                    using var content = new MultipartFormDataContent();
                    using var stream = model.ImageFile.OpenReadStream();
                    content.Add(new StreamContent(stream), "file", model.ImageFile.FileName);
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
        public async Task<IActionResult> EditEvent(int id, UpdateEventRequestDto model, IFormFile? NewImage) // GAP 4: Added IFormFile
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
                // GAP 4: Upload new image if one was provided
                if (NewImage != null && NewImage.Length > 0)
                {
                    using var content = new MultipartFormDataContent();
                    using var stream = NewImage.OpenReadStream();
                    content.Add(new StreamContent(stream), "file", NewImage.FileName);
                    await _api.PostFileAsync($"/api/v1/events/{id}/upload-image", content);
                }

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
        // CATEGORY METHODS
        // ==========================================

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(int eventId, CreateCategoryRequestDto model)
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                var errors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                TempData["Error"] = "Validation failed: " + errors;
                return RedirectToAction(nameof(ManageCategories), new { id = eventId });
            }

            var response = await _api.PostAsync($"/api/v1/events/{eventId}/categories", model);

            if (response.IsSuccessStatusCode)
                TempData["Success"] = "Category added successfully!";
            else
            {
                TempData["Error"] = $"API Error: {response.StatusCode}";
            }

            return RedirectToAction(nameof(ManageCategories), new { id = eventId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int categoryId, int eventId)
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Account");

            var response = await _api.DeleteAsync($"/api/v1/categories/{categoryId}");

            if (response.IsSuccessStatusCode)
                TempData["Success"] = "Category deleted successfully.";
            else
                TempData["Error"] = "Failed to delete category. It might have participants registered.";

            return RedirectToAction(nameof(ManageCategories), new { id = eventId });
        }

        // ==========================================
        // ENTRY & RESULT METHODS
        // ==========================================

        public async Task<IActionResult> ManageEntries(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Account");

            var ev = await _api.GetAsync<EventDetailsDto>($"/api/v1/events/{id}");
            if (ev == null) return NotFound();

            var entries = await _api.GetAsync<List<EntryWithParticipantDto>>($"/api/v1/events/{id}/entries");

            ViewBag.EventId = id;
            ViewBag.EventName = ev.Name;

            return View(entries ?? new List<EntryWithParticipantDto>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEntry(int entryId, int eventId, string status, int? startNumber)
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Account");

            var dto = new UpdateEntryRequestDto
            {
                Status = status,
                StartNumber = startNumber
            };

            var response = await _api.PutAsync($"/api/v1/entries/{entryId}", dto);

            if (response.IsSuccessStatusCode)
                TempData["Success"] = "Entry updated successfully!";
            else
                TempData["Error"] = "Failed to update entry.";

            return RedirectToAction(nameof(ManageEntries), new { id = eventId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadResult(int entryId, int eventId, CreateResultRequestDto model)
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid result data.";
                return RedirectToAction(nameof(ManageEntries), new { id = eventId });
            }

            var response = await _api.PostAsync($"/api/v1/entries/{entryId}/result", model);

            if (response.IsSuccessStatusCode)
                TempData["Success"] = "Result uploaded successfully!";
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"API Error: {response.StatusCode}. {errorMsg}";
            }

            return RedirectToAction(nameof(ManageEntries), new { id = eventId });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;
using RaceDay.DTOs;

namespace RaceDay.Mvc.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly ApiClient _api;

        public ParticipantController(ApiClient api)
        {
            _api = api;
        }

        // GET: /Participant/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            if (HttpContext.Session.GetString("UserRole") != "Participant")
                return RedirectToAction("Login", "Account");

            var profile = await _api.GetAsync<UserProfileResponseDto>("/api/v1/users/me");

            if (profile == null)
                return RedirectToAction("Index", "Home");

            // AUTO-FIX: If they don't have a participant profile, create one on the fly!
            if (profile.ParticipantID == null)
            {
                var names = (profile.FullName ?? "Unknown User").Split(' ', 2);

                var createDto = new CreateParticipantRequestDto
                {
                    UserID = profile.UserId, // Matches the DTO
                    FirstName = names[0],
                    LastName = names.Length > 1 ? names[1] : "",
                    Gender = "M",
                    DateOfBirth = DateTime.Today.AddYears(-20)
                };

                await _api.PostAsync("/api/v1/participants", createDto);

                // Fetch the profile again now that it exists
                profile = await _api.GetAsync<UserProfileResponseDto>("/api/v1/users/me");
            }

            if (profile?.ParticipantID == null)
                return RedirectToAction("Index", "Home");

            // Fetch entries for this participant
            var entries = await _api.GetAsync<List<EntryResponseDto>>($"/api/v1/participants/{profile.ParticipantID}/entries");

            // Fetch events for the names
            var events = await _api.GetAsync<List<EventResponseDto>>("/api/v1/events");

            ViewBag.Events = events?.ToDictionary(e => e.EventID) ?? new Dictionary<int, EventResponseDto>();
            ViewBag.ParticipantID = profile.ParticipantID.Value;

            return View(entries ?? new List<EntryResponseDto>());
        }

        // POST: /Participant/EnterEvent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnterEvent(int eventId, int? categoryId)
        {
            if (HttpContext.Session.GetString("UserRole") != "Participant")
                return RedirectToAction("Login", "Account");

            var profile = await _api.GetAsync<UserProfileResponseDto>("/api/v1/users/me");

            if (profile?.ParticipantID == null)
                return RedirectToAction("Index", "Home");

            var dto = new CreateEntryRequestDto
            {
                ParticipantID = profile.ParticipantID.Value,
                EventID = eventId,
                CategoryID = categoryId
            };

            var response = await _api.PostAsync("/api/v1/entries", dto);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "You have successfully entered the event!";
                return RedirectToAction(nameof(Dashboard));
            }

            TempData["Error"] = "Failed to enter event. You may have already entered.";
            return RedirectToAction("Details", "Events", new { id = eventId });
        }

        // POST: /Participant/Withdraw
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(int entryId)
        {
            if (HttpContext.Session.GetString("UserRole") != "Participant")
                return RedirectToAction("Login", "Account");

            var response = await _api.DeleteAsync($"/api/v1/entries/{entryId}");

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "You have successfully withdrawn from the event.";
            }
            else
            {
                TempData["Error"] = "Failed to withdraw. The event might have started or results are published.";
            }

            return RedirectToAction(nameof(Dashboard));
        }

        // GET: /Participant/MyResults
        public async Task<IActionResult> MyResults()
        {
            if (HttpContext.Session.GetString("UserRole") != "Participant")
                return RedirectToAction("Login", "Account");

            var profile = await _api.GetAsync<UserProfileResponseDto>("/api/v1/users/me");

            if (profile?.ParticipantID == null)
                return RedirectToAction("Index", "Home");

            // Call the new API endpoint we added
            var results = await _api.GetAsync<List<ResultResponseDto>>($"/api/v1/participants/{profile.ParticipantID}/results");

            return View(results ?? new List<ResultResponseDto>());
        }
    }
}

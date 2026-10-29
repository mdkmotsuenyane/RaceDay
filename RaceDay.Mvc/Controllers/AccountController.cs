using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
using RaceDay.DTOs;
using System.Net.Http.Json;
using System.Net.Http;
using System.IO;
using System;

namespace RaceDay.Mvc.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiClient _api;

        public AccountController(ApiClient api)
        {
            _api = api;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("JwtToken") != null)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var dto = new LoginRequestDto { Email = model.Email, Password = model.Password };
            var response = await _api.PostAsync("/api/v1/auth/login", dto);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
                HttpContext.Session.SetString("JwtToken", result!.Token);

                var profile = await _api.GetAsync<UserProfileResponseDto>("/api/v1/users/me");

                if (profile != null)
                {
                    HttpContext.Session.SetString("UserRole", profile.Role);
                    HttpContext.Session.SetString("UserName", profile.FullName ?? "User");
                    HttpContext.Session.SetString("ProfileImageUrl", profile.ProfileImageUrl ?? "");
                }

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var dto = new RegisterRequestDto
            {
                Email = model.Email,
                Password = model.Password,
                FullName = model.FullName,
                Role = model.Role,
                Phone = model.Phone,
                DateOfBirth = model.DateOfBirth
            };

            var response = await _api.PostAsync("/api/v1/auth/register", dto);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Account created! Please log in.";
                return RedirectToAction(nameof(Login));
            }

            ModelState.AddModelError("", "Registration failed. Email might already exist.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // ==========================================
        // PROFILE METHODS
        // ==========================================

        // GET: /Account/Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (HttpContext.Session.GetString("JwtToken") == null)
                return RedirectToAction("Login");

            var profile = await _api.GetAsync<UserProfileResponseDto>("/api/v1/users/me");
            if (profile == null) return NotFound();

            var model = new ProfileViewModel
            {
                Email = profile.Email,
                Role = profile.Role,
                ProfileImageUrl = profile.ProfileImageUrl,
                FullName = profile.FullName ?? "",
                Phone = profile.Phone,
                EmergencyContact = profile.EmergencyContact
            };

            return View(model);
        }

        // POST: /Account/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (HttpContext.Session.GetString("JwtToken") == null)
                return RedirectToAction("Login");

            if (!ModelState.IsValid) return View(model);

            try
            {
                // 1. Update text fields
                var updateDto = new UpdateUserProfileRequestDto
                {
                    FullName = model.FullName,
                    Phone = model.Phone,
                    EmergencyContact = model.EmergencyContact
                };

                var updateResponse = await _api.PutAsync("/api/v1/users/me", updateDto);

                if (!updateResponse.IsSuccessStatusCode)
                {
                    TempData["Error"] = $"Failed to update basic info. API said: {updateResponse.StatusCode}";
                    return RedirectToAction(nameof(Profile));
                }

                // 2. Upload profile picture if one was selected
                if (model.ProfileImage != null && model.ProfileImage.Length > 0)
                {
                    using var content = new MultipartFormDataContent();
                    using var stream = model.ProfileImage.OpenReadStream();
                    content.Add(new StreamContent(stream), "file", model.ProfileImage.FileName);

                    var imgResponse = await _api.PostFileAsync("/api/v1/users/me/profile-picture", content);

                    if (!imgResponse.IsSuccessStatusCode)
                    {
                        var errorMsg = await imgResponse.Content.ReadAsStringAsync();
                        TempData["Error"] = $"Failed to upload image. API said: {errorMsg}";
                        return RedirectToAction(nameof(Profile));
                    }
                }

                // 3. Fetch the updated profile to update the session
                var updatedProfile = await _api.GetAsync<UserProfileResponseDto>("/api/v1/users/me");
                if (updatedProfile != null)
                {
                    HttpContext.Session.SetString("UserName", updatedProfile.FullName ?? "User");
                    HttpContext.Session.SetString("ProfileImageUrl", updatedProfile.ProfileImageUrl ?? "");
                }

                TempData["Success"] = "Profile updated successfully!";
                return RedirectToAction(nameof(Profile));
            }
            catch (Exception ex)
            {
                // Catch API crashes so the MVC app doesn't close!
                TempData["Error"] = $"Connection to API failed: {ex.Message}";
                return RedirectToAction(nameof(Profile));
            }
        }

        // POST: /Account/DeleteProfilePicture
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProfilePicture()
        {
            if (HttpContext.Session.GetString("JwtToken") == null)
                return RedirectToAction("Login");

            try
            {
                await _api.PostAsync("/api/v1/users/me/profile-picture/delete", new { });
                HttpContext.Session.SetString("ProfileImageUrl", "");
                TempData["Success"] = "Profile picture removed successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Connection to API failed: {ex.Message}";
            }

            return RedirectToAction(nameof(Profile));
        }
    }
}

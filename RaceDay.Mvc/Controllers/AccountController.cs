using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
using RaceDay.DTOs;

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
    }
}

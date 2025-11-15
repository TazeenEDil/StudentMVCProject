using Microsoft.AspNetCore.Mvc;
using StudentManagement.DTOs.Auth;    // your DTOs (RegisterRequestDto, LoginRequestDto, AuthResponseDto)
using StudentManagement.Interfaces.Services;

namespace StudentManagement.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequestDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var result = await _authService.RegisterAsync(dto);

            if (result == null)
            {
                // registration failed (e.g. username exists)
                ModelState.AddModelError(string.Empty, "Registration failed — username/email may already exist.");
                return View(dto);
            }

            // Do NOT auto-login. Redirect to Login page as requested.
            TempData["Info"] = "Registration successful. Please log in.";
            return RedirectToAction(nameof(Login));
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequestDto dto)

        {
            if (!ModelState.IsValid) return View(dto);

            var result = await _authService.LoginAsync(dto);
            if (result == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid credentials.");
                return View(dto);
            }

            // store JWT in a secure HTTP-only cookie named "jwt"
            Response.Cookies.Append("jwt", result.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // set true for https
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(8)
            });

            // Redirect based on role to Students list by default
            if (result.Role == "Admin")
                return RedirectToAction("Index", "Students");
            else
                return RedirectToAction("Index", "Students");
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            return RedirectToAction("Index", "Home");
        }

    }
}

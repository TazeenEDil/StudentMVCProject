using Microsoft.AspNetCore.Mvc;
using StudentManagement.DTOs.Auth;
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

            try
            {
                var result = await _authService.RegisterAsync(dto);

                if (result == null)
                {
                    ModelState.AddModelError(string.Empty,
                        "Registration failed — email or username may already exist.");
                    return View(dto);
                }

                TempData["Info"] = "Registration successful. Please log in.";
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty,
                    "An unexpected error occurred during registration. Please try again.");

                // Optional: log the exception somewhere (Serilog, Console, etc.)
                Console.WriteLine(ex);

                return View(dto);
            }
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

            try
            {
                var result = await _authService.LoginAsync(dto);

                if (result == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid credentials.");
                    return View(dto);
                }

                // Store JWT securely
                Response.Cookies.Append("jwt", result.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(8)
                });

                return RedirectToAction("Index", "Students");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty,
                    "An error occurred while trying to log in. Please try again.");

                Console.WriteLine(ex); // log exception

                return View(dto);
            }
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            try
            {
                Response.Cookies.Delete("jwt");
            }
            catch (Exception ex)
            {
                // Handle cookie deletion failure (rare but possible)
                Console.WriteLine(ex);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}

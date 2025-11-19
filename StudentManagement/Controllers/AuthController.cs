using Microsoft.AspNetCore.Mvc;
using StudentManagement.DTOs.Auth;
using StudentManagement.Interfaces.Services;
using StudentManagement.Interfaces.Persistence;

namespace StudentManagement.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            ILogger<AuthController> logger,
            IAuthService authService,
            IUserRepository userRepository)
        {
            _logger = logger;
            _authService = authService;
            _userRepository = userRepository;
        }

        [HttpGet]
        public IActionResult Register()
        {
            _logger.LogInformation("GET /Auth/Register accessed");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid registration attempt for email {Email}", dto.Email);
                return View(dto);
            }

            try
            {
                _logger.LogInformation("Attempting registration for email {Email}", dto.Email);

                var result = await _authService.RegisterAsync(dto);
                TempData["Info"] = "Registration successful! Check your email for login credentials.";

                _logger.LogInformation("Registration successful for email {Email}", dto.Email);
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for email {Email}", dto.Email);
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            _logger.LogInformation("GET /Auth/Login accessed");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid login attempt for {Email}", dto.Email);
                return View(dto);
            }

            try
            {
                _logger.LogInformation("Attempting login for {Email}", dto.Email);

                var result = await _authService.LoginAsync(dto);

                if (result == null)
                {
                    _logger.LogWarning("Login failed for {Email}: Invalid credentials", dto.Email);
                    ModelState.AddModelError(string.Empty, "Invalid email or password.");
                    return View(dto);
                }

                Response.Cookies.Append("jwt", result.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(480)
                });

                _logger.LogInformation("Login successful for {Email}", dto.Email);
                return RedirectToAction("Index", "Students");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login exception for {Email}", dto.Email);
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        public IActionResult Logout()
        {
            try
            {
                _logger.LogInformation("User attempting logout");
                Response.Cookies.Delete("jwt");
                _logger.LogInformation("Logout successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout failed");
            }

            return RedirectToAction("Index", "Home");
        }
    }
}

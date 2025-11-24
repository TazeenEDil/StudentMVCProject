using Microsoft.AspNetCore.Mvc;
using StudentManagement.DTOs.Auth;
using StudentManagement.Interfaces.Services;
using StudentManagement.Interfaces.Persistence;
using StudentManagement.Helpers;

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

        // REGISTER (GET)
        [HttpGet]
        public IActionResult Register()
        {
            _logger.LogInformation("GET /Auth/Register accessed");
            return View();
        }

        // REGISTER (POST)
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
                _logger.LogInformation("Validating email existence for {Email}...", dto.Email);

                // Validate that email exists and can receive mail
                if (!await EmailValidator.IsRealEmailAsync(dto.Email))
                {
                    ModelState.AddModelError("Email", "This email address does not exist or cannot receive mail.");
                    _logger.LogWarning("Registration blocked: Email {Email} does not exist or is invalid", dto.Email);
                    return View(dto);
                }

                _logger.LogInformation("Email validated successfully. Attempting registration for {Email}", dto.Email);

                // Proceed with registration - this will also save to DB
                var result = await _authService.RegisterAsync(dto);

                TempData["Info"] = "Registration successful! Check your email for login credentials.";
                _logger.LogInformation("User successfully registered and saved to database: {Email}", dto.Email);
                
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for email {Email}", dto.Email);
                
                // If it's an email-related error, show specific message
                if (ex.Message.Contains("email", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("Email", ex.Message);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Registration failed. Please try again.");
                }
                
                return View(dto);
            }
        }

        // LOGIN (GET)
        [HttpGet]
        public IActionResult Login()
        {
            _logger.LogInformation("GET /Auth/Login accessed");
            return View();
        }

        // LOGIN (POST)
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
                _logger.LogInformation("Validating email existence for login attempt: {Email}", dto.Email);

                // NEW: Validate email exists before checking credentials
                // This prevents login attempts with non-existent emails
                if (!await EmailValidator.IsRealEmailAsync(dto.Email))
                {
                    _logger.LogWarning("Login blocked: Email {Email} does not exist or is invalid", dto.Email);
                    // Use generic error message to avoid revealing whether email exists in system
                    ModelState.AddModelError(string.Empty, "Invalid email or password.");
                    return View(dto);
                }

                _logger.LogInformation("Email validated. Attempting authentication for {Email}", dto.Email);

                var result = await _authService.LoginAsync(dto);

                if (result == null)
                {
                    _logger.LogWarning("Login failed for {Email}: Invalid credentials", dto.Email);
                    ModelState.AddModelError(string.Empty, "Invalid email or password.");
                    return View(dto);
                }

                // Set authentication cookie
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
                
                // Use generic error message for security
                ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
                return View(dto);
            }
        }

        // LOGOUT
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
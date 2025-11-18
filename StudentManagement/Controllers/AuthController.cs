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

        public AuthController(IAuthService authService, IUserRepository userRepository)
        {
            _authService = authService;
            _userRepository = userRepository;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register() => View();

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequestDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                // Register user (no confirmation logic)
                var result = await _authService.RegisterAsync(dto);

                TempData["Info"] = "Registration successful! Check your email for login credentials.";
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                Console.WriteLine(ex);
                return View(dto);
            }
        }


        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login() => View();

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequestDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                var result = await _authService.LoginAsync(dto);

                if (result == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid email or password.");
                    return View(dto);
                }

                // Store JWT in secure cookie
                Response.Cookies.Append("jwt", result.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(480)
                });

                return RedirectToAction("Index", "Students");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                Console.WriteLine(ex);
                return View(dto);
            }
        }


        // LOGOUT
        public IActionResult Logout()
        {
            try
            {
                Response.Cookies.Delete("jwt");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}

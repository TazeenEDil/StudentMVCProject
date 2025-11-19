using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Interfaces.Persistence;
using StudentManagement.Models;

namespace StudentManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserRepository userRepository,
            IStudentRepository studentRepository,
            ILogger<UsersController> logger)
        {
            _userRepository = userRepository;
            _studentRepository = studentRepository;
            _logger = logger;
        }

        // GET: /Users/Index - Shows all users 
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Admin accessing all users list");

            try
            {
                var users = await _userRepository.GetAllAsync();
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users list");
                TempData["Error"] = "Something went wrong while loading users.";
                return View(new List<User>());
            }
        }

        // GET: /Users/Details/5 - View details
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            _logger.LogInformation("Viewing user details for ID {Id}", id);

            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("User ID {Id} not found", id);
                    return NotFound();
                }

                
                if (user.Role == "Student")
                {
                    var student = await _studentRepository.GetByEmailAsync(user.Email);
                    ViewBag.StudentProfile = student;
                }

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading details for user ID {Id}", id);
                TempData["Error"] = "Unable to load user details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Users/EditStudent/5 - Edit student user
        [HttpGet]
        public async Task<IActionResult> EditStudent(int id)
        {
            _logger.LogInformation("GET /Users/EditStudent/{Id}", id);

            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("User ID {Id} not found for edit", id);
                    return NotFound();
                }

                
                if (user.Role != "Student")
                {
                    _logger.LogWarning("Attempted to edit non-student user ID {Id}", id);
                    TempData["Error"] = "Only student users can be edited.";
                    return RedirectToAction(nameof(Index));
                }

                var student = await _studentRepository.GetByEmailAsync(user.Email);
                if (student == null)
                {
                    _logger.LogWarning("Student profile not found for user ID {Id}", id);
                    TempData["Error"] = "Student profile not found.";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.User = user;
                return View(student);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to load edit page for ID {Id}", id);
                TempData["Error"] = "Unable to load student.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Users/EditStudent - Update student user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStudent(Student student)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid edit data for student ID {Id}", student.Id);
                return View(student);
            }

            try
            {
                _logger.LogInformation("Updating student ID {Id}", student.Id);

                var updated = await _studentRepository.UpdateAsync(student);

                if (updated == null)
                {
                    _logger.LogWarning("Student ID {Id} not found for update", student.Id);
                    return NotFound();
                }

                TempData["Success"] = "Student updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating student ID {Id}", student.Id);
                ModelState.AddModelError("", "Unable to update student. Try again.");
                return View(student);
            }
        }

        // GET: /Users/DeleteStudent/5 - Delete confirmation
        [HttpGet]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            _logger.LogInformation("GET /Users/DeleteStudent/{Id}", id);

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User ID {Id} not found for delete", id);
                return NotFound();
            }

           
            if (user.Role != "Student")
            {
                _logger.LogWarning("Attempted to delete non-student user ID {Id}", id);
                TempData["Error"] = "Only student users can be deleted.";
                return RedirectToAction(nameof(Index));
            }

            var student = await _studentRepository.GetByEmailAsync(user.Email);
            ViewBag.User = user;
            return View(student);
        }

        // POST: /Users/DeleteStudent/5 - Delete student user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStudentConfirmed(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null || user.Role != "Student")
                {
                    TempData["Error"] = "Only student users can be deleted.";
                    return RedirectToAction(nameof(Index));
                }

                var deleted = await _userRepository.DeleteUserAndStudentAsync(id);
                if (!deleted)
                {
                    TempData["Error"] = "User not found or could not be deleted.";
                }
                else
                {
                    TempData["Success"] = "Student user deleted successfully.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user ID {Id}", id);
                TempData["Error"] = "An error occurred while deleting the user.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
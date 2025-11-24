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
            _logger.LogInformation("GET /Users/EditStudent/{Id} (User ID)", id);

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

                _logger.LogInformation("Sending Student to view - Student ID: {StudentId}, Name: {Name}, Email: {Email}",
                    student.Id, student.Name, student.Email);

                ViewBag.User = user;
                ViewBag.OriginalEmail = student.Email;  

                return View(student);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to load edit page for user ID {Id}", id);
                TempData["Error"] = "Unable to load student.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Users/EditStudent - Update student user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStudent(Student student, string originalEmail)
        {
            _logger.LogInformation("POST EditStudent - Original Email: {OriginalEmail}, New Email: {NewEmail}, Name: {Name}",
                originalEmail, student?.Email, student?.Name);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid");
                var userForView = await _userRepository.GetByEmailAsync(originalEmail);
                ViewBag.User = userForView;
                ViewBag.OriginalEmail = originalEmail;
                return View(student);
            }

            try
            {
                // Fetch existing student by email
                var existingStudent = await _studentRepository.GetByEmailAsync(originalEmail);

                if (existingStudent == null)
                {
                    _logger.LogWarning("Student with original email {Email} not found", originalEmail);
                    TempData["Error"] = "Student not found.";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation("Found existing student: ID {Id}, Name {Name}",
                    existingStudent.Id, existingStudent.Name);

                // Get the associated user by email
                var user = await _userRepository.GetByEmailAsync(originalEmail);

                if (user == null)
                {
                    _logger.LogWarning("User not found for original email: {Email}", originalEmail);
                    TempData["Error"] = "Associated user not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if email is being changed and if new email already exists
                if (originalEmail != student.Email)
                {
                    var emailExists = await _studentRepository.GetByEmailAsync(student.Email);
                    if (emailExists != null)
                    {
                        _logger.LogWarning("Email {Email} already exists", student.Email);
                        ModelState.AddModelError("Email", "This email is already in use.");
                        ViewBag.User = user;
                        ViewBag.OriginalEmail = originalEmail;
                        return View(student);
                    }
                }

                // Apply updates to student
                existingStudent.Name = student.Name;
                existingStudent.Email = student.Email;
                existingStudent.RegistrationNumber = student.RegistrationNumber;
                existingStudent.DateOfBirth = student.DateOfBirth;
                existingStudent.Department = student.Department;

                // Update user email if changed
                if (user.Email != student.Email)
                {
                    _logger.LogInformation("Email changed from {OldEmail} to {NewEmail}",
                        user.Email, student.Email);
                    user.Email = student.Email;
                    await _userRepository.UpdateAsync(user);
                }

                // Save student changes
                await _studentRepository.UpdateAsync(existingStudent);

                TempData["Success"] = "Student updated successfully.";
                _logger.LogInformation("Student updated successfully: ID {Id}, Email: {Email}",
                    existingStudent.Id, existingStudent.Email);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating student with original email {Email}", originalEmail);
                TempData["Error"] = "Unable to update student. Please try again.";

                var userForView = await _userRepository.GetByEmailAsync(originalEmail);
                ViewBag.User = userForView;
                ViewBag.OriginalEmail = originalEmail;
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
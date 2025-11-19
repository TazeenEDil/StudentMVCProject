using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.DTOs.Students;
using StudentManagement.Interfaces.Persistence;
using StudentManagement.Interfaces.Services;
using StudentManagement.Models;


namespace StudentManagement.Controllers
{
    [Authorize]
    public class StudentsController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<StudentsController> _logger;

        public StudentsController(IStudentService studentService, IUserRepository userRepository, ILogger<StudentsController> logger)
        {
            _studentService = studentService;
            _userRepository = userRepository;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Fetching all students");

            try
            {
                var students = await _studentService.GetAllStudentsAsync();
                return View(students);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading students list");
                TempData["Error"] = "Something went wrong while loading students.";
                return View(new List<Student>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            _logger.LogInformation("Fetching student details for ID {Id}", id);

            try
            {
                var student = await _studentService.GetStudentByIdAsync(id);
                if (student == null)
                {
                    _logger.LogWarning("Student ID {Id} not found", id);
                    return NotFound();
                }

                return View(student);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading details for student ID {Id}", id);
                TempData["Error"] = "Unable to load student details.";
                return RedirectToAction(nameof(Index));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            _logger.LogInformation("GET /Students/Create accessed");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateStudentDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid create student attempt");
                return View(dto);
            }

            try
            {
                _logger.LogInformation("Creating student {Name}", dto.Name);
                await _studentService.CreateStudentAsync(dto);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create student");
                ModelState.AddModelError("", "Failed to create student. Try again.");
                return View(dto);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            _logger.LogInformation("GET /Students/Edit/{Id}", id);

            try
            {
                var student = await _studentService.GetStudentByIdAsync(id);
                if (student == null)
                {
                    _logger.LogWarning("Student ID {Id} not found for edit", id);
                    return NotFound();
                }

                return View(student);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to load edit page for ID {Id}", id);
                TempData["Error"] = "Unable to load student.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Student student)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid edit data for student ID {Id}", student.Id);
                return View(student);
            }

            try
            {
                _logger.LogInformation("Updating student ID {Id}", student.Id);

                var updated = await _studentService.UpdateStudentAsync(new UpdateStudentDto
                {
                    Id = student.Id,
                    Name = student.Name,
                    Email = student.Email,
                    RegistrationNumber = student.RegistrationNumber,
                    DateOfBirth = student.DateOfBirth,
                    Department = student.Department
                });

                if (updated == null)
                {
                    _logger.LogWarning("Student ID {Id} not found for update", student.Id);
                    return NotFound();
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating student ID {Id}", student.Id);
                ModelState.AddModelError("", "Unable to update student. Try again.");
                return View(student);
            }
        }

        // GET: /Students/Delete/5
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("GET /Students/Delete/{Id}", id);

            var student = await _studentService.GetStudentByIdAsync(id);
            if (student == null)
            {
                _logger.LogWarning("Student ID {Id} not found for delete", id);
                return NotFound();
            }

            return View(student);
        }

        // POST: /Students/Delete/5
        
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var deleted = await _userRepository.DeleteUserAndStudentAsync(id);
                if (!deleted)
                {
                    TempData["Error"] = "User not found or could not be deleted.";
                }
                else
                {
                    TempData["Success"] = "User and student profile deleted successfully.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user ID {Id}", id);
                TempData["Error"] = "An error occurred while deleting the user.";
            }

            return RedirectToAction("Index");
        }





    }
}

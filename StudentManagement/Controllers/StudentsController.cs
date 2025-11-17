using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.DTOs.Students;
using StudentManagement.Interfaces.Services;
using StudentManagement.Models;

namespace StudentManagement.Controllers
{
    [Authorize]
    public class StudentsController : Controller
    {
        private readonly IStudentService _studentService;
        public StudentsController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        // GET: /Students
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var students = await _studentService.GetAllStudentsAsync();
                return View(students);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                TempData["Error"] = "Something went wrong while loading students.";
                return View(new List<Student>());
            }
        }

        // GET: /Students/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var student = await _studentService.GetStudentByIdAsync(id);
                if (student == null) return NotFound();
                return View(student);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                TempData["Error"] = "Unable to load student details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Students/Create
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Students/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateStudentDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            try
            {
                await _studentService.CreateStudentAsync(dto);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                ModelState.AddModelError("", "Failed to create student. Try again.");
                return View(dto);
            }
        }

        // GET: /Students/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var student = await _studentService.GetStudentByIdAsync(id);
                if (student == null) return NotFound();
                return View(student);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                TempData["Error"] = "Unable to load student.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Student student)
        {
            if (!ModelState.IsValid) return View(student);

            try
            {
                var updated = await _studentService.UpdateStudentAsync(new UpdateStudentDto
                {
                    Id = student.Id,
                    Name = student.Name,
                    Email = student.Email,
                    RegistrationNumber = student.RegistrationNumber,
                    DateOfBirth = student.DateOfBirth,
                    Department = student.Department
                });

                if (updated == null) return NotFound();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                ModelState.AddModelError("", "Unable to update student. Try again.");
                return View(student);
            }
        }

        // GET: /Students/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var student = await _studentService.GetStudentByIdAsync(id);
                if (student == null) return NotFound();
                return View(student);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                TempData["Error"] = "Unable to load delete page.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Students/DeleteConfirmed
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var success = await _studentService.DeleteStudentAsync(id);
                if (!success) TempData["Error"] = "Failed to delete student.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                TempData["Error"] = "An error occurred while deleting the student.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}

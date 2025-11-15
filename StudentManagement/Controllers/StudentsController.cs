using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.DTOs.Students;
using StudentManagement.Interfaces;
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
            var students = await _studentService.GetAllStudentsAsync();
            return View(students);
        }

        // GET: /Students/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var students = await _studentService.GetStudentByIdAsync(id);
            if (students == null) return NotFound();
            return View(students);
        }

        // GET: /Students/Create
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create() => View();

        // POST: /Students/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateStudentDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            await _studentService.CreateStudentAsync(dto);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Students/Edit/5
        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var student = await _studentService.GetStudentByIdAsync(id);
            if (student == null) return NotFound();
            return View(student);
        }

        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Student student)
        {
            if (!ModelState.IsValid) return View(student);

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


        // GET: Students/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var student = await _studentService.GetStudentByIdAsync(id);
            if (student == null) return NotFound();
            return View(student);
        }

        // POST: Students/DeleteConfirmed

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _studentService.DeleteStudentAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}

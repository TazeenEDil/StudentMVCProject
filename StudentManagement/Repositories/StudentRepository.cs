using Microsoft.EntityFrameworkCore;
using StudentManagement.Data;
using StudentManagement.Interfaces.Persistence;
using StudentManagement.Models;

namespace StudentManagement.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<StudentRepository> _logger;

        public StudentRepository(AppDbContext context, ILogger<StudentRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Student> CreateAsync(Student student)
        {
            if (student == null)
            {
                _logger.LogWarning("CreateAsync called with null student");
                throw new ArgumentNullException(nameof(student), "Student cannot be null");
            }

            _logger.LogInformation("Creating student {@Student}", student);

            try
            {
                await _context.Students.AddAsync(student);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Student created successfully with ID: {Id}", student.Id);

                return student;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while creating student {@Student}", student);
                throw new Exception("Database error occurred while creating student.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating student {@Student}", student);
                throw;
            }
        }


        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting student with ID {Id}", id);

            try
            {
                var entity = await _context.Students.FindAsync(id);

                if (entity == null)
                {
                    _logger.LogWarning("Attempted to delete non-existing student with ID {Id}", id);
                    return false;
                }

                _context.Students.Remove(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Student deleted successfully with ID {Id}", id);
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while deleting student with ID {Id}", id);
                throw;   // keep original exception, do not wrap
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting student with ID {Id}", id);
                throw;   // preserve trace
            }
        }

        public async Task<IEnumerable<Student>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all students");

            try
            {
                var result = await _context.Students.AsNoTracking().ToListAsync();

                _logger.LogInformation("Retrieved {Count} students", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all students");
                throw new Exception("Error occurred while retrieving all students.", ex);
            }
        }

        public async Task<Student> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching student by ID {Id}", id);

            try
            {
                var student = await _context.Students.FindAsync(id);

                if (student == null)
                    _logger.LogWarning("Student not found with ID {Id}", id);
                else
                    _logger.LogInformation("Student found {@Student}", student);

                return student;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving student with ID {Id}", id);
                throw new Exception("Error occurred while retrieving student by ID.", ex);
            }
        }

        public async Task<Student> GetByEmailAsync(string email)
        {
            _logger.LogInformation("Fetching student by email {Email}", email);

            try
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == email);

                if (student == null)
                    _logger.LogWarning("Student not found with email {Email}", email);
                else
                    _logger.LogInformation("Student found {@Student}", student);

                return student;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving student with email {Email}", email);
                throw new Exception("Error occurred while retrieving student by email.", ex);
            }
        }

        public async Task<Student> UpdateAsync(Student student)
        {
            if (student == null)
            {
                _logger.LogWarning("UpdateAsync called with null student");
                throw new ArgumentNullException(nameof(student), "Student cannot be null");
            }

            _logger.LogInformation("Updating student {@Student}", student);

            try
            {
                var existingStudent = await _context.Students.FindAsync(student.Id);
                if (existingStudent == null)
                {
                    _logger.LogWarning("Student not found: {Id}", student.Id);
                    return null;
                }

                // Update only allowed fields
                existingStudent.Name = student.Name;
                existingStudent.Email = student.Email;
                existingStudent.RegistrationNumber = student.RegistrationNumber;
                existingStudent.DateOfBirth = student.DateOfBirth;
                existingStudent.Department = student.Department;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Student updated successfully: {Id}", student.Id);
                return existingStudent;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error while updating student {@Student}", student);
                throw new Exception("Concurrency error occurred while updating student.", ex);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while updating student {@Student}", student);
                throw new Exception("Database error occurred while updating student.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating student {@Student}", student);
                throw new Exception("Unexpected error occurred while updating student.", ex);
            }
        }


    }
}
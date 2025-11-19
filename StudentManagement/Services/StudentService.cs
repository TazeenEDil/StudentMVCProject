using StudentManagement.DTOs.Students;
using StudentManagement.Interfaces.Persistence;
using StudentManagement.Interfaces.Services;
using StudentManagement.Models;

namespace StudentManagement.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly ILogger<StudentService> _logger;

        public StudentService(IStudentRepository studentRepository, ILogger<StudentService> logger)
        {
            _studentRepository = studentRepository;
            _logger = logger;
        }

        public async Task<Student> CreateStudentAsync(CreateStudentDto dto)
        {
            _logger.LogInformation("Creating student {@DTO}", dto);

            try
            {
                var student = new Student
                {
                    Name = dto.Name,
                    Email = dto.Email,
                    RegistrationNumber = dto.RegistrationNumber,
                    DateOfBirth = dto.DateOfBirth,
                    Department = dto.Department
                };

                var created = await _studentRepository.CreateAsync(student);

                _logger.LogInformation("Student created with ID {Id}", created.Id);

                return created;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating student {@DTO}", dto);
                throw;
            }
        }

        public async Task<bool> DeleteStudentAsync(int id)
        {
            _logger.LogInformation("Deleting student with ID {Id}", id);

            try
            {
                var result = await _studentRepository.DeleteAsync(id);

                if (!result)
                {
                    _logger.LogWarning("Delete failed. Student not found: {Id}", id);
                    return false;
                }

                _logger.LogInformation("Student deleted successfully {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting student {Id}", id);
                throw;
            }
        }


        public async Task<IEnumerable<Student>> GetAllStudentsAsync()
        {
            _logger.LogInformation("Fetching all students");

            try
            {
                var students = await _studentRepository.GetAllAsync();
                _logger.LogInformation("Retrieved {Count} students", students.Count());
                return students;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all students");
                throw;
            }
        }

        public async Task<Student> GetStudentByIdAsync(int id)
        {
            _logger.LogInformation("Fetching student by ID {Id}", id);

            try
            {
                var student = await _studentRepository.GetByIdAsync(id);

                if (student == null)
                    _logger.LogWarning("Student not found {Id}", id);

                return student!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching student {Id}", id);
                throw;
            }
        }

        public async Task<Student> UpdateStudentAsync(UpdateStudentDto dto)
        {
            _logger.LogInformation("Updating student {@DTO}", dto);

            try
            {
                var existing = await _studentRepository.GetByIdAsync(dto.Id);

                if (existing == null)
                {
                    _logger.LogWarning("Student not found for update {Id}", dto.Id);
                    throw new Exception($"Student with ID {dto.Id} not found.");
                }

                existing.Name = dto.Name;
                existing.Email = dto.Email;
                existing.RegistrationNumber = dto.RegistrationNumber;
                existing.DateOfBirth = dto.DateOfBirth;
                existing.Department = dto.Department;

                var updated = await _studentRepository.UpdateAsync(existing);

                _logger.LogInformation("Student updated successfully {Id}", updated.Id);

                return updated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating student {Id}", dto.Id);
                throw;
            }
        }
    }
}

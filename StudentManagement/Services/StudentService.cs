using StudentManagement.DTOs.Students;
using StudentManagement.Interfaces.Persistence;
using StudentManagement.Interfaces.Services;
using StudentManagement.Models;

namespace StudentManagement.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;
        public StudentService(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        public async Task<Student> CreateStudentAsync(CreateStudentDto dto)
        {
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

                return await _studentRepository.CreateAsync(student);
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while creating the student.", ex);
            }
        }

        public async Task<bool> DeleteStudentAsync(int id)
        {
            try
            {
                return await _studentRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while deleting the student with ID {id}.", ex);
            }
        }

        public async Task<IEnumerable<Student>> GetAllStudentsAsync()
        {
            try
            {
                return await _studentRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while retrieving all students.", ex);
            }
        }

        public async Task<Student> GetStudentByIdAsync(int id)
        {
            try
            {
                var student = await _studentRepository.GetByIdAsync(id);
                if (student == null)
                    throw new Exception($"Student with ID {id} not found.");

                return student;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while retrieving the student with ID {id}.", ex);
            }
        }

        public async Task<Student> UpdateStudentAsync(UpdateStudentDto dto)
        {
            try
            {
                var existing = await _studentRepository.GetByIdAsync(dto.Id);
                if (existing == null)
                    throw new Exception($"Student with ID {dto.Id} not found.");

                existing.Name = dto.Name;
                existing.Email = dto.Email;
                existing.RegistrationNumber = dto.RegistrationNumber;
                existing.DateOfBirth = dto.DateOfBirth;
                existing.Department = dto.Department;

                return await _studentRepository.UpdateAsync(existing);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while updating the student with ID {dto.Id}.", ex);
            }
        }
    }
}

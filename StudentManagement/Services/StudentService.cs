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

        public async Task<bool> DeleteStudentAsync(int id)
        {
            return await _studentRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<Student>> GetAllStudentsAsync()
        {
            return await _studentRepository.GetAllAsync();
        }

        public async Task<Student> GetStudentByIdAsync(int id)
        {
            return await _studentRepository.GetByIdAsync(id);
        }

        public async Task<Student> UpdateStudentAsync(UpdateStudentDto dto)
        {
            var existing = await _studentRepository.GetByIdAsync(dto.Id);
            if (existing == null) return null;

            existing.Name = dto.Name;
            existing.Email = dto.Email;
            existing.RegistrationNumber = dto.RegistrationNumber;
            existing.DateOfBirth = dto.DateOfBirth;
            existing.Department = dto.Department;

            return await _studentRepository.UpdateAsync(existing);
        }


    }
}

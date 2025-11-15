using StudentManagement.DTOs.Students;
using StudentManagement.Models;

namespace StudentManagement.Interfaces.Services
{
    public interface IStudentService
    {
        Task<IEnumerable<Student>> GetAllStudentsAsync();
        Task<Student> GetStudentByIdAsync(int id);
        Task<Student> CreateStudentAsync(CreateStudentDto dto);
        Task<Student> UpdateStudentAsync(UpdateStudentDto dto);
        Task<bool> DeleteStudentAsync(int id);
    }
}

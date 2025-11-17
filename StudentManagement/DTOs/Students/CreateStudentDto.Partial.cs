
namespace StudentManagement.DTOs.Students
{
    public partial class CreateStudentDto : StudentDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string RegistrationNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Department { get; set; }
    }


}


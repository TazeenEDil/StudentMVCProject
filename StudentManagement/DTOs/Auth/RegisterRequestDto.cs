using System.ComponentModel.DataAnnotations;

namespace StudentManagement.DTOs.Auth
{
    public class RegisterRequestDto
    {

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
            ErrorMessage = "Password must be at least 8 characters long, with at least one uppercase letter, one lowercase letter, and one number.")]
        public string Password { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Role { get; set; }

        
    }
}
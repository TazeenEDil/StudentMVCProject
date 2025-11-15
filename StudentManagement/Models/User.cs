using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = null!;

        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string PasswordHash { get; set; } = null!;

        [Required]
        public string Role { get; set; } // "Admin" or "Student" (stored as string in DB)
    }
}

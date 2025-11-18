namespace StudentManagement.Models
{
    public class JwtSettings
        {
            public string Key { get; set; } = null!;
            public string Issuer { get; set; } = null!;
            public string Audience { get; set; } = null!;
            public int ExpiryMinutes { get; set; }
            public string ConfirmationBaseUrl { get; set; } = null!;
        }
    }

using BCrypt.Net;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StudentManagement.DTOs.Auth;
using StudentManagement.Interfaces.Services;
using StudentManagement.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace StudentManagement.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            IUserRepository userRepository,
            IOptions<JwtSettings> jwtOptions,
            IEmailService emailService)
        {
            _userRepository = userRepository;
            _jwtSettings = jwtOptions.Value;
            _emailService = emailService;
        }

        // =====================
        // LOGIN
        // =====================
        public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);

            if (user == null)
                return null;

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return null;

            // Email confirmation removed completely

            var token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                Role = user.Role
            };
        }

        // =====================
        // REGISTER
        // =====================
        public async Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto dto)
        {
            var exists = await _userRepository.GetByEmailAsync(dto.Email);
            if (exists != null)
                throw new Exception("User already exists");

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role
            };

            await _userRepository.CreateAsync(user);

            // ===========================
            // SEND WELCOME EMAIL
            // ===========================
            string emailBody = $@"
                <h2>Congratulations {user.Username}!</h2>
                <p>You have registered successfully in <b>Student Management System</b>.</p>

                <h3>Your Login Credentials</h3>
                <p><b>Email:</b> {user.Email}</p>
                <p><b>Password:</b> {dto.Password}</p>

                <p>Please keep this information safe.</p>
                <br/>
                <p>Regards,<br/>Student Management Team</p>
            ";

            await _emailService.SendEmailAsync(
                user.Email,
                "Registration Successful",
                emailBody
            );

            return new AuthResponseDto
            {
                Email = user.Email,
                Role = user.Role,
                Token = null // Login later
            };
        }

        // =====================
        // GENERATE JWT TOKEN
        // =====================
        private string GenerateJwtToken(User user)
        {
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(token);
        }
    }
}

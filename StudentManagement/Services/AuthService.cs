using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StudentManagement.DTOs.Auth;
using StudentManagement.Interfaces.Services;
using StudentManagement.Interfaces.Persistence;
using StudentManagement.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace StudentManagement.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IEmailService _emailService;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IStudentRepository studentRepository,
            IOptions<JwtSettings> jwtOptions,
            IEmailService emailService,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _studentRepository = studentRepository;
            _jwtSettings = jwtOptions.Value;
            _emailService = emailService;
            _logger = logger;
        }

        // LOGIN
        public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto dto)
        {
            _logger.LogInformation("Login attempt for email {Email}", dto.Email);

            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed. No user found with email {Email}", dto.Email);
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed. Invalid password for {Email}", dto.Email);
                return null;
            }

            _logger.LogInformation("Login successful for user {Email}", dto.Email);

            var token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                Role = user.Role
            };
        }

        // REGISTER
        public async Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto dto)
        {
            _logger.LogInformation("Registering new user {Email}", dto.Email);

            var exists = await _userRepository.GetByEmailAsync(dto.Email);
            if (exists != null)
            {
                _logger.LogWarning("Registration failed: user already exists {Email}", dto.Email);
                throw new Exception("User already exists");
            }

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role
            };

            await _userRepository.CreateAsync(user);

            _logger.LogInformation("User registered successfully {Email}", dto.Email);

            //Student entry automatically if role is Student
            if (dto.Role == "Student")
            {
                var student = new Student
                {
                    Name = dto.Username,
                    Email = dto.Email,
                    RegistrationNumber = Guid.NewGuid().ToString().Substring(0, 8),
                    DateOfBirth = DateTime.Now,   // you can adjust
                    Department = "Not Assigned"   // editable later
                };

                // Use repository method
                await _studentRepository.CreateAsync(student);

                _logger.LogInformation("Student profile created for {Email}", dto.Email);
            }

            // WELCOME EMAIL
            await _emailService.SendEmailAsync(
                user.Email,
                "Registration Successful",
                $@"<h2>Congratulations {user.Username}!</h2>
                   <p>You have registered successfully.</p>
                   <p><strong>Email:</strong> {user.Email}</p>
                   <p><strong>Password:</strong> {dto.Password}</p>"
            );

            _logger.LogInformation("Welcome email sent to {Email}", user.Email);

            return new AuthResponseDto
            {
                Email = user.Email,
                Role = user.Role,
                Token = null
            };
        }

        // GENERATE JWT TOKEN
        private string GenerateJwtToken(User user)
        {
            _logger.LogInformation("Generating JWT for user {Id}", user.Id);

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

            _logger.LogInformation("JWT generated for user {Id}", user.Id);

            return handler.WriteToken(token);
        }
    }
}

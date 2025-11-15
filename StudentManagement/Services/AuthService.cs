using BCrypt.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StudentManagement.DTOs.Auth;
using StudentManagement.Interfaces.Persistence;
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
        private readonly IConfiguration _configuration;

        public AuthService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        // -------------------------------
        // LOGIN USING EMAIL + PASSWORD
        // -------------------------------
        public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto dto)
        {
            // Fetch by EMAIL
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null)
                return null;

            // Validate password
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return null;

            // Generate JWT token
            var token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                Role = user.Role
            };
        }

        // -------------------------------
        // REGISTER USER
        // -------------------------------
        public async Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto dto)
        {
            // Check if email already exists
            var existing = await _userRepository.GetByEmailAsync(dto.Email);
            if (existing != null)
                return null;

            // Create user
            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role
            };

            var createdUser = await _userRepository.CreateAsync(user);

            // NOTE: Controller decides whether to auto-login or not.
            var token = GenerateJwtToken(createdUser);

            return new AuthResponseDto
            {
                Token = token,
                Email = createdUser.Email,
                Role = createdUser.Role
            };
        }

        // -------------------------------
        // GENERATE JWT TOKEN
        // -------------------------------
        private string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(jwtKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),  // user id
                new Claim(ClaimTypes.Email, user.Email),                   // login by email
                new Claim(ClaimTypes.Name, user.Username),                 // display username
                new Claim(ClaimTypes.Role, user.Role)                      // roles
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8),
                Issuer = jwtIssuer,
                Audience = jwtAudience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}

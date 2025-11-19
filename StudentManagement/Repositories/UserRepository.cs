using Microsoft.EntityFrameworkCore;
using StudentManagement.Data;
using StudentManagement.Interfaces.Persistence;
using StudentManagement.Models;

namespace StudentManagement.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(AppDbContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User> CreateAsync(User user)
        {
            _logger.LogInformation("Creating user {@User}", user);
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("User created successfully with Id {Id}", user.Id);
            return user;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            _logger.LogInformation("Fetching user by email: {Email}", email);
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user == null)
                _logger.LogWarning("No user found for email: {Email}", email);
            return user;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching user by Id: {Id}", id);
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                _logger.LogWarning("No user found for Id: {Id}", id);
            return user;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all users");
            try
            {
                var users = await _context.Users.AsNoTracking().ToListAsync();
                _logger.LogInformation("Retrieved {Count} users", users.Count);
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all users");
                throw new Exception("Error occurred while retrieving all users.", ex);
            }
        }

        public async Task<User> UpdateAsync(User user)
        {
            _logger.LogInformation("Updating user {@User}", user);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("User updated successfully: {Id}", user.Id);
            return user;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting user with ID {Id}", id);
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User with ID {Id} not found", id);
                return false;
            }
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("User with ID {Id} deleted successfully", id);
            return true;
        }

        public async Task<bool> DeleteUserAndStudentAsync(int userId)
        {
            _logger.LogInformation("Deleting user and associated student for user ID {UserId}", userId);

            // Fetch the user
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", userId);
                return false;
            }

            // Delete associated student, if exists
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Email == user.Email);
            if (student != null)
            {
                _context.Students.Remove(student);
                _logger.LogInformation("Associated student found and removed for email {Email}", user.Email);
            }

            // Delete user
            _context.Users.Remove(user);

            await _context.SaveChangesAsync();
            _logger.LogInformation("User and associated student deleted successfully for ID {UserId}", userId);
            return true;
        }
    }
}
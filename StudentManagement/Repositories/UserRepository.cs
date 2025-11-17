using Microsoft.EntityFrameworkCore;
using StudentManagement.Data;
using StudentManagement.Interfaces.Persistence;
using StudentManagement.Models;

namespace StudentManagement.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User> CreateAsync(User user)
        {
            try
            {
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
                return user;
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Database error occurred while creating the user.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error occurred while creating the user.", ex);
            }
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            try
            {
                return await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
            }
            catch (InvalidOperationException ex)
            {
                // SingleOrDefault throws this if more than one user found
                throw new Exception($"Multiple users found with email: {email}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error occurred while fetching user by email.", ex);
            }
        }
    }
}

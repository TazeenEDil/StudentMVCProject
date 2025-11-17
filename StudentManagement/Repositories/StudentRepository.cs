using Microsoft.EntityFrameworkCore;
using StudentManagement.Data;
using StudentManagement.Interfaces.Persistence;
using StudentManagement.Models;

namespace StudentManagement.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly AppDbContext _context;
        public StudentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Student> CreateAsync(Student student)
        {
            try
            {
                await _context.Students.AddAsync(student);
                await _context.SaveChangesAsync();
                return student;
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Database error while creating student.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error occurred while creating student.", ex);
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var entity = await _context.Students.FindAsync(id);

                if (entity == null)
                    return false;

                _context.Students.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Database error while deleting student.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error occurred while deleting student.", ex);
            }
        }

        public async Task<IEnumerable<Student>> GetAllAsync()
        {
            try
            {
                return await _context.Students.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while retrieving all students.", ex);
            }
        }

        public async Task<Student> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Students.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while retrieving student by ID.", ex);
            }
        }

        public async Task<Student> UpdateAsync(Student student)
        {
            try
            {
                _context.Students.Update(student);
                await _context.SaveChangesAsync();
                return student;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new Exception("Concurrency error occurred while updating student.", ex);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Database error occurred while updating student.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error occurred while updating student.", ex);
            }
        }
    }
}

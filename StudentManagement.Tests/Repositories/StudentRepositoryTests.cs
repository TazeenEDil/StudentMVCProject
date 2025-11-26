using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using StudentManagement.Data;
using StudentManagement.Models;
using StudentManagement.Repositories;

namespace StudentManagement.Tests.Repositories
{
    public class StudentRepositoryTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly StudentRepository _repository;
        private readonly Mock<ILogger<StudentRepository>> _loggerMock;

        public StudentRepositoryTests()
        {
            // Arrange: Set up in-memory database with unique name for each test
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _loggerMock = new Mock<ILogger<StudentRepository>>();
            _repository = new StudentRepository(_context, _loggerMock.Object);
        }

        // Cleanup after each test
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_ShouldAddStudent_WhenStudentIsValid()
        {
            // Arrange: Create a new student
            var student = new Student
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
                RegistrationNumber = "REG001",
                DateOfBirth = new DateTime(2000, 1, 1),
                Department = "Computer Science"
            };

            // Act: Call the method to add student
            var result = await _repository.CreateAsync(student);

            // Assert: Verify student was added
            Assert.NotNull(result);
            Assert.True(result.Id > 0); // ID should be assigned
            Assert.Equal("John Doe", result.Name);
            Assert.Equal("john.doe@example.com", result.Email);

            // Verify it exists in database
            var savedStudent = await _context.Students.FindAsync(result.Id);
            Assert.NotNull(savedStudent);
            Assert.Equal("John Doe", savedStudent.Name);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenStudentIsNull()
        {
            // Act & Assert: Verify exception is thrown
            await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _repository.CreateAsync(null)
            );
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_ShouldReturnStudent_WhenIdIsValid()
        {
            // Arrange: Add a student to database
            var student = new Student
            {
                Name = "Jane Smith",
                Email = "jane.smith@example.com",
                RegistrationNumber = "REG002",
                DateOfBirth = new DateTime(1999, 5, 15),
                Department = "Mathematics"
            };
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            // Act: Retrieve student by ID
            var result = await _repository.GetByIdAsync(student.Id);

            // Assert: Verify correct student is returned
            Assert.NotNull(result);
            Assert.Equal(student.Id, result.Id);
            Assert.Equal("Jane Smith", result.Name);
            Assert.Equal("jane.smith@example.com", result.Email);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenIdIsInvalid()
        {
            // Act: Try to get non-existent student
            var result = await _repository.GetByIdAsync(999);

            // Assert: Should return null
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenIdIsNegative()
        {
            // Act: Try with negative ID
            var result = await _repository.GetByIdAsync(-1);

            // Assert: Should return null
            Assert.Null(result);
        }

        #endregion

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllStudents_WhenStudentsExist()
        {
            // Arrange: Add multiple students
            var students = new List<Student>
            {
                new Student
                {
                    Name = "Student One",
                    Email = "student1@example.com",
                    RegistrationNumber = "REG101",
                    DateOfBirth = new DateTime(2000, 1, 1),
                    Department = "CS"
                },
                new Student
                {
                    Name = "Student Two",
                    Email = "student2@example.com",
                    RegistrationNumber = "REG102",
                    DateOfBirth = new DateTime(2000, 2, 2),
                    Department = "Math"
                },
                new Student
                {
                    Name = "Student Three",
                    Email = "student3@example.com",
                    RegistrationNumber = "REG103",
                    DateOfBirth = new DateTime(2000, 3, 3),
                    Department = "Physics"
                }
            };

            _context.Students.AddRange(students);
            await _context.SaveChangesAsync();

            // Act: Get all students
            var result = await _repository.GetAllAsync();

            // Assert: Verify count and content
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
            Assert.Contains(result, s => s.Name == "Student One");
            Assert.Contains(result, s => s.Name == "Student Two");
            Assert.Contains(result, s => s.Name == "Student Three");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoStudentsExist()
        {
            // Act: Get all students from empty database
            var result = await _repository.GetAllAsync();

            // Assert: Should return empty collection
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnCorrectCount_AfterAddingStudents()
        {
            // Arrange: Start with empty database
            var initialResult = await _repository.GetAllAsync();
            Assert.Empty(initialResult);

            // Add 5 students
            for (int i = 1; i <= 5; i++)
            {
                var student = new Student
                {
                    Name = $"Student {i}",
                    Email = $"student{i}@example.com",
                    RegistrationNumber = $"REG{i:000}",
                    DateOfBirth = new DateTime(2000, 1, i),
                    Department = "CS"
                };
                _context.Students.Add(student);
            }
            await _context.SaveChangesAsync();

            // Act: Get all students
            var result = await _repository.GetAllAsync();

            // Assert: Should have exactly 5 students
            Assert.Equal(5, result.Count());
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ShouldUpdateStudent_WhenStudentExists()
        {
            // Arrange: Add a student
            var student = new Student
            {
                Name = "Old Name",
                Email = "old.email@example.com",
                RegistrationNumber = "REG200",
                DateOfBirth = new DateTime(2000, 1, 1),
                Department = "Old Department"
            };
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            // Modify student details
            student.Name = "Updated Name";
            student.Email = "new.email@example.com";
            student.Department = "New Department";

            // Act: Update the student
            var result = await _repository.UpdateAsync(student);

            // Assert: Verify updates
            Assert.NotNull(result);
            Assert.Equal("Updated Name", result.Name);
            Assert.Equal("new.email@example.com", result.Email);
            Assert.Equal("New Department", result.Department);

            // Verify in database
            var updatedStudent = await _context.Students.FindAsync(student.Id);
            Assert.Equal("Updated Name", updatedStudent.Name);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnNull_WhenStudentDoesNotExist()
        {
            // Arrange: Create student that doesn't exist in DB
            var student = new Student
            {
                Id = 999,
                Name = "Non Existent",
                Email = "ghost@example.com",
                RegistrationNumber = "REG999",
                DateOfBirth = new DateTime(2000, 1, 1),
                Department = "Unknown"
            };

            // Act: Try to update non-existent student
            var result = await _repository.UpdateAsync(student);

            // Assert: Should return null
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateOnlyAllowedFields_AndNotChangeId()
        {
            // Arrange: Add a student
            var student = new Student
            {
                Name = "Original Name",
                Email = "original@example.com",
                RegistrationNumber = "REG300",
                DateOfBirth = new DateTime(2000, 5, 5),
                Department = "Original Dept"
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            var originalId = student.Id;

            // Create a separate object for update
            var updatedStudent = new Student
            {
                Id = student.Id, // Keep original ID
                Name = "New Name",
                Email = "new@example.com",
                RegistrationNumber = "NEWREG",
                DateOfBirth = new DateTime(2001, 1, 1),
                Department = "New Dept"
            };

            // Act
            var result = await _repository.UpdateAsync(updatedStudent);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(originalId, result.Id);  

            Assert.Equal("New Name", result.Name);
            Assert.Equal("new@example.com", result.Email);
            Assert.Equal("NEWREG", result.RegistrationNumber);
            Assert.Equal(new DateTime(2001, 1, 1), result.DateOfBirth);
            Assert.Equal("New Dept", result.Department);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_ShouldRemoveStudent_WhenIdIsValid()
        {
            // Arrange: Add a student
            var student = new Student
            {
                Name = "To Be Deleted",
                Email = "delete.me@example.com",
                RegistrationNumber = "REG400",
                DateOfBirth = new DateTime(2000, 1, 1),
                Department = "CS"
            };
            _context.Students.Add(student);
            await _context.SaveChangesAsync();
            var studentId = student.Id;

            // Verify student exists
            var existingStudent = await _context.Students.FindAsync(studentId);
            Assert.NotNull(existingStudent);

            // Act: Delete the student
            var result = await _repository.DeleteAsync(studentId);

            // Assert: Delete should succeed
            Assert.True(result);

            // Verify student no longer exists
            var deletedStudent = await _context.Students.FindAsync(studentId);
            Assert.Null(deletedStudent);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenIdIsInvalid()
        {
            // Act: Try to delete non-existent student
            var result = await _repository.DeleteAsync(999);

            // Assert: Should return false
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDecreaseStudentCount_AfterDeletion()
        {
            // Arrange: Add 3 students
            for (int i = 1; i <= 3; i++)
            {
                var student = new Student
                {
                    Name = $"Student {i}",
                    Email = $"student{i}@example.com",
                    RegistrationNumber = $"REG{i}",
                    DateOfBirth = new DateTime(2000, 1, 1),
                    Department = "CS"
                };
                _context.Students.Add(student);
            }
            await _context.SaveChangesAsync();

            // Verify initial count
            var initialCount = await _context.Students.CountAsync();
            Assert.Equal(3, initialCount);

            // Act: Delete one student
            var firstStudent = await _context.Students.FirstAsync();
            var deleteResult = await _repository.DeleteAsync(firstStudent.Id);

            // Assert: Count should decrease
            Assert.True(deleteResult);
            var finalCount = await _context.Students.CountAsync();
            Assert.Equal(2, finalCount);
        }

        #endregion
        
        #region GetByEmailAsync Tests

        [Fact]
        public async Task GetByEmailAsync_ShouldReturnStudent_WhenEmailExists()
        {
            // Arrange: Add a student
            var student = new Student
            {
                Name = "Email Test",
                Email = "test.email@example.com",
                RegistrationNumber = "REG500",
                DateOfBirth = new DateTime(2000, 1, 1),
                Department = "CS"
            };
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            // Act: Search by email
            var result = await _repository.GetByEmailAsync("test.email@example.com");

            // Assert: Should find the student
            Assert.NotNull(result);
            Assert.Equal("Email Test", result.Name);
            Assert.Equal("test.email@example.com", result.Email);
        }

        [Fact]
        public async Task GetByEmailAsync_ShouldReturnNull_WhenEmailDoesNotExist()
        {
            // Act: Search for non-existent email
            var result = await _repository.GetByEmailAsync("nonexistent@example.com");

            // Assert: Should return null
            Assert.Null(result);
        }

        #endregion
    }
}
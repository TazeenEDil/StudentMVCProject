using Microsoft.Extensions.Logging;
using Moq;
using StudentManagement.DTOs.Students;
using StudentManagement.Interfaces.Persistence;
using StudentManagement.Models;
using StudentManagement.Services;

namespace StudentManagement.Tests.Services
{
    public class StudentServiceTests
    {
        private readonly Mock<IStudentRepository> _repositoryMock;
        private readonly Mock<ILogger<StudentService>> _loggerMock;
        private readonly StudentService _service;

        public StudentServiceTests()
        {
            _repositoryMock = new Mock<IStudentRepository>();
            _loggerMock = new Mock<ILogger<StudentService>>();
            _service = new StudentService(_repositoryMock.Object, _loggerMock.Object);
        }

        #region CreateStudentAsync Tests

        [Fact]
        public async Task CreateStudentAsync_ShouldCreateStudent_WhenDtoIsValid()
        {
            // Arrange: Create DTO
            var dto = new CreateStudentDto
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
                RegistrationNumber = "REG001",
                DateOfBirth = new DateTime(2000, 1, 1),
                Department = "Computer Science"
            };

            var expectedStudent = new Student
            {
                Id = 1,
                Name = dto.Name,
                Email = dto.Email,
                RegistrationNumber = dto.RegistrationNumber,
                DateOfBirth = dto.DateOfBirth,
                Department = dto.Department
            };

            // Setup mock to return created student
            _repositoryMock
                .Setup(r => r.CreateAsync(It.IsAny<Student>()))
                .ReturnsAsync(expectedStudent);

            // Act: Call service method
            var result = await _service.CreateStudentAsync(dto);

            // Assert: Verify result
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("John Doe", result.Name);
            Assert.Equal("john.doe@example.com", result.Email);

            // Verify repository was called once
            _repositoryMock.Verify(
                r => r.CreateAsync(It.Is<Student>(s =>
                    s.Name == dto.Name &&
                    s.Email == dto.Email &&
                    s.RegistrationNumber == dto.RegistrationNumber
                )),
                Times.Once
            );
        }

        [Fact]
        public async Task CreateStudentAsync_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange: Setup mock to throw exception
            var dto = new CreateStudentDto
            {
                Name = "Test",
                Email = "test@example.com",
                RegistrationNumber = "REG999",
                DateOfBirth = new DateTime(2000, 1, 1),
                Department = "CS"
            };

            _repositoryMock
                .Setup(r => r.CreateAsync(It.IsAny<Student>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert: Verify exception is thrown
            await Assert.ThrowsAsync<Exception>(
                async () => await _service.CreateStudentAsync(dto)
            );
        }

        [Fact]
        public async Task CreateStudentAsync_ShouldMapDtoToStudent_Correctly()
        {
            // Arrange
            var dto = new CreateStudentDto
            {
                Name = "Jane Smith",
                Email = "jane@example.com",
                RegistrationNumber = "REG123",
                DateOfBirth = new DateTime(1999, 5, 15),
                Department = "Mathematics"
            };

            Student capturedStudent = null;

            _repositoryMock
                .Setup(r => r.CreateAsync(It.IsAny<Student>()))
                .Callback<Student>(s => capturedStudent = s)
                .ReturnsAsync((Student s) => { s.Id = 5; return s; });

            // Act
            await _service.CreateStudentAsync(dto);

            // Assert: Verify mapping
            Assert.NotNull(capturedStudent);
            Assert.Equal(dto.Name, capturedStudent.Name);
            Assert.Equal(dto.Email, capturedStudent.Email);
            Assert.Equal(dto.RegistrationNumber, capturedStudent.RegistrationNumber);
            Assert.Equal(dto.DateOfBirth, capturedStudent.DateOfBirth);
            Assert.Equal(dto.Department, capturedStudent.Department);
        }

        #endregion

        #region GetStudentByIdAsync Tests

        [Fact]
        public async Task GetStudentByIdAsync_ShouldReturnStudent_WhenIdIsValid()
        {
            // Arrange: Setup mock to return student
            var expectedStudent = new Student
            {
                Id = 1,
                Name = "John Doe",
                Email = "john@example.com",
                RegistrationNumber = "REG001",
                DateOfBirth = new DateTime(2000, 1, 1),
                Department = "CS"
            };

            _repositoryMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(expectedStudent);

            // Act: Get student by ID
            var result = await _service.GetStudentByIdAsync(1);

            // Assert: Verify result
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("John Doe", result.Name);

            // Verify repository was called
            _repositoryMock.Verify(r => r.GetByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetStudentByIdAsync_ShouldReturnNull_WhenIdIsInvalid()
        {
            // Arrange: Setup mock to return null
            _repositoryMock
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((Student)null);

            // Act: Try to get non-existent student
            var result = await _service.GetStudentByIdAsync(999);

            // Assert: Should return null
            Assert.Null(result);
            _repositoryMock.Verify(r => r.GetByIdAsync(999), Times.Once);
        }

        [Fact]
        public async Task GetStudentByIdAsync_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange: Setup mock to throw exception
            _repositoryMock
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act & Assert: Verify exception is propagated
            await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetStudentByIdAsync(1)
            );
        }

        #endregion

        #region GetAllStudentsAsync Tests

        [Fact]
        public async Task GetAllStudentsAsync_ShouldReturnAllStudents_WhenStudentsExist()
        {
            // Arrange: Setup mock with multiple students
            var students = new List<Student>
            {
                new Student
                {
                    Id = 1,
                    Name = "Student One",
                    Email = "student1@example.com",
                    RegistrationNumber = "REG001",
                    DateOfBirth = new DateTime(2000, 1, 1),
                    Department = "CS"
                },
                new Student
                {
                    Id = 2,
                    Name = "Student Two",
                    Email = "student2@example.com",
                    RegistrationNumber = "REG002",
                    DateOfBirth = new DateTime(2000, 2, 2),
                    Department = "Math"
                },
                new Student
                {
                    Id = 3,
                    Name = "Student Three",
                    Email = "student3@example.com",
                    RegistrationNumber = "REG003",
                    DateOfBirth = new DateTime(2000, 3, 3),
                    Department = "Physics"
                }
            };

            _repositoryMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(students);

            // Act: Get all students
            var result = await _service.GetAllStudentsAsync();

            // Assert: Verify results
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
            Assert.Contains(result, s => s.Name == "Student One");
            Assert.Contains(result, s => s.Name == "Student Two");
            Assert.Contains(result, s => s.Name == "Student Three");

            _repositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllStudentsAsync_ShouldReturnEmptyList_WhenNoStudentsExist()
        {
            // Arrange: Setup mock to return empty list
            _repositoryMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Student>());

            // Act: Get all students
            var result = await _service.GetAllStudentsAsync();

            // Assert: Should return empty collection
            Assert.NotNull(result);
            Assert.Empty(result);
            _repositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllStudentsAsync_ShouldReturnCorrectCount()
        {
            // Arrange: Create 10 students
            var students = Enumerable.Range(1, 10).Select(i => new Student
            {
                Id = i,
                Name = $"Student {i}",
                Email = $"student{i}@example.com",
                RegistrationNumber = $"REG{i:000}",
                DateOfBirth = new DateTime(2000, 1, 1),
                Department = "CS"
            }).ToList();

            _repositoryMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(students);

            // Act
            var result = await _service.GetAllStudentsAsync();

            // Assert: Should have exactly 10 students
            Assert.Equal(10, result.Count());
        }

        #endregion

        #region UpdateStudentAsync Tests

        [Fact]
        public async Task UpdateStudentAsync_ShouldUpdateStudent_WhenStudentExists()
        {
            // Arrange: Create existing student
            var existingStudent = new Student
            {
                Id = 1,
                Name = "Old Name",
                Email = "old@example.com",
                RegistrationNumber = "REG001",
                DateOfBirth = new DateTime(2000, 1, 1),
                Department = "Old Dept"
            };

            var updateDto = new UpdateStudentDto
            {
                Id = 1,
                Name = "Updated Name",
                Email = "new@example.com",
                RegistrationNumber = "REG001",
                DateOfBirth = new DateTime(2000, 1, 1),
                Department = "New Dept"
            };

            var updatedStudent = new Student
            {
                Id = 1,
                Name = updateDto.Name,
                Email = updateDto.Email,
                RegistrationNumber = updateDto.RegistrationNumber,
                DateOfBirth = updateDto.DateOfBirth,
                Department = updateDto.Department
            };

            // Setup mocks
            _repositoryMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingStudent);

            _repositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<Student>()))
                .ReturnsAsync(updatedStudent);

            // Act: Update student
            var result = await _service.UpdateStudentAsync(updateDto);

            // Assert: Verify updates
            Assert.NotNull(result);
            Assert.Equal("Updated Name", result.Name);
            Assert.Equal("new@example.com", result.Email);
            Assert.Equal("New Dept", result.Department);

            // Verify methods were called
            _repositoryMock.Verify(r => r.GetByIdAsync(1), Times.Once);
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Student>()), Times.Once);
        }

        [Fact]
        public async Task UpdateStudentAsync_ShouldThrowException_WhenStudentDoesNotExist()
        {
            // Arrange: Setup mock to return null (student not found)
            var updateDto = new UpdateStudentDto
            {
                Id = 999,
                Name = "Non Existent",
                Email = "ghost@example.com",
                RegistrationNumber = "REG999",
                DateOfBirth = new DateTime(2000, 1, 1),
                Department = "Unknown"
            };

            _repositoryMock
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((Student)null);

            // Act & Assert: Should throw exception
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.UpdateStudentAsync(updateDto)
            );

            Assert.Contains("not found", exception.Message);
            _repositoryMock.Verify(r => r.GetByIdAsync(999), Times.Once);
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Student>()), Times.Never);
        }

        [Fact]
        public async Task UpdateStudentAsync_ShouldUpdateAllFields_Correctly()
        {
            // Arrange
            var existingStudent = new Student
            {
                Id = 5,
                Name = "Original",
                Email = "original@example.com",
                RegistrationNumber = "OLD",
                DateOfBirth = new DateTime(1990, 1, 1),
                Department = "OldDept"
            };

            var updateDto = new UpdateStudentDto
            {
                Id = 5,
                Name = "NewName",
                Email = "newemail@example.com",
                RegistrationNumber = "NEW123",
                DateOfBirth = new DateTime(2000, 12, 31),
                Department = "NewDepartment"
            };

            Student capturedStudent = null;

            _repositoryMock
                .Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(existingStudent);

            _repositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<Student>()))
                .Callback<Student>(s => capturedStudent = s)
                .ReturnsAsync((Student s) => s);

            // Act
            await _service.UpdateStudentAsync(updateDto);

            // Assert: Verify all fields were updated
            Assert.NotNull(capturedStudent);
            Assert.Equal("NewName", capturedStudent.Name);
            Assert.Equal("newemail@example.com", capturedStudent.Email);
            Assert.Equal("NEW123", capturedStudent.RegistrationNumber);
            Assert.Equal(new DateTime(2000, 12, 31), capturedStudent.DateOfBirth);
            Assert.Equal("NewDepartment", capturedStudent.Department);
        }

        #endregion

        #region DeleteStudentAsync Tests

        [Fact]
        public async Task DeleteStudentAsync_ShouldReturnTrue_WhenStudentIsDeleted()
        {
            // Arrange: Setup mock to return true (successful deletion)
            _repositoryMock
                .Setup(r => r.DeleteAsync(1))
                .ReturnsAsync(true);

            // Act: Delete student
            var result = await _service.DeleteStudentAsync(1);

            // Assert: Should return true
            Assert.True(result);
            _repositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteStudentAsync_ShouldReturnFalse_WhenStudentNotFound()
        {
            // Arrange: Setup mock to return false (student not found)
            _repositoryMock
                .Setup(r => r.DeleteAsync(999))
                .ReturnsAsync(false);

            // Act: Try to delete non-existent student
            var result = await _service.DeleteStudentAsync(999);

            // Assert: Should return false
            Assert.False(result);
            _repositoryMock.Verify(r => r.DeleteAsync(999), Times.Once);
        }

        [Fact]
        public async Task DeleteStudentAsync_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange: Setup mock to throw exception
            _repositoryMock
                .Setup(r => r.DeleteAsync(It.IsAny<int>()))
                .ThrowsAsync(new Exception("Database error during deletion"));

            // Act & Assert: Should propagate exception
            await Assert.ThrowsAsync<Exception>(
                async () => await _service.DeleteStudentAsync(1)
            );
        }

        [Fact]
        public async Task DeleteStudentAsync_ShouldCallRepository_WithCorrectId()
        {
            // Arrange
            const int studentId = 42;
            _repositoryMock
                .Setup(r => r.DeleteAsync(studentId))
                .ReturnsAsync(true);

            // Act
            await _service.DeleteStudentAsync(studentId);

            // Assert: Verify correct ID was passed
            _repositoryMock.Verify(
                r => r.DeleteAsync(42),
                Times.Once,
                "Repository should be called with ID 42"
            );
        }

        #endregion
    }
}
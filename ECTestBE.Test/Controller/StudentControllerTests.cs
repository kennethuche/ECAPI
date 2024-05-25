using AutoMapper;
using ECTest.Service.Interface;
using ECTest.Service.Models;
using ECTestBE.Controllers;
using ECTestBE.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace ECTestBE.Test.Controller
{
    public class StudentControllerTests
    {
        private readonly Mock<IStudentRepository> _mockStudentRepository;
        private readonly Mock<ILogger<StudentController>> _mockLogger;
        private readonly Mock<IMapper> _mockMapper;
        private readonly StudentController _controller;


        public StudentControllerTests()
        {
            _mockStudentRepository = new Mock<IStudentRepository>();
            _mockLogger = new Mock<ILogger<StudentController>>();
            _mockMapper = new Mock<IMapper>();
            _controller = new StudentController(_mockLogger.Object, _mockMapper.Object, _mockStudentRepository.Object);
        }

        [Fact]
        public void GetStudents_ReturnsOkResult_WithListOfStudents()
        {
            // Arrange
            var students = new List<Student> { new Student { Id = Guid.NewGuid(), FullName = "Kenneth Uche", Email = "info@meetkennethuche.com" } };
            _mockStudentRepository.Setup(repo => repo.GetStudents()).Returns(students.AsQueryable());

            // Act
            var result = _controller.GetStudents();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
         
            Assert.Equal(200, okResult.StatusCode);
           
        }
        [Fact]
        public async Task GetStudent_ReturnsOkResult_WithStudent()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var student = new Student { Id = studentId, FullName = "Kenneth Uche", Email = "info@meetkennethuche.com" };
            _mockStudentRepository.Setup(repo => repo.GetStudentByIdAsync(studentId)).ReturnsAsync(student);

            // Act
            var result = await _controller.GetStudent(studentId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<Student>(okResult.Value);
            Assert.Equal(studentId, returnValue.Id);
        }

        [Fact]
        public async Task PostStudent_ReturnsCreatedResult_WithStudentId()
        {
            // Arrange
            var studentVm = new StudentVm
            {
                FullName = "Kenneth Uche",
                Email = "info@meetkennethuche.com",
                Courses = new List<CourseVm>
        {
            new CourseVm
            {
                StartDate = new DateTimeOffset(new DateTime(2023, 6, 5)),
                EndDate = new DateTimeOffset(new DateTime(2023, 6, 9))
            }
        }
            };

            var student = new Student
            {
                Id = Guid.NewGuid(),
                FullName = "Kenneth Uche",
                Email = "info@meetkennethuche.com",
                Courses = new List<Course>
        {
            new Course
            {
                StartDate = studentVm.Courses.First().StartDate,
                EndDate = studentVm.Courses.First().EndDate
            }
        }
            };

            _mockMapper.Setup(m => m.Map<Student>(studentVm)).Returns(student);
            _mockStudentRepository.Setup(repo => repo.CreateStudentAsync(student)).ReturnsAsync(student);

            // Act
            var result = await _controller.PostStudent(studentVm);

            // Assert
            var createdResult = Assert.IsType<CreatedResult>(result.Result);
            Assert.Equal(201, createdResult.StatusCode);
            Assert.Equal("Success", createdResult.Location);
        }


        [Fact]
        public async Task PutStudent_ReturnsOkResult_WithUpdatedStudent()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var studentVm = new StudentVm { FullName = "Kenneth Uche", Email = "info@meetkennethuche.com" };
            var student = new Student { Id = studentId, FullName = "Kenneth Uche", Email = "info@meetkennethuche.com" };
            _mockStudentRepository.Setup(repo => repo.GetStudentByIdAsync(studentId)).ReturnsAsync(student);
            _mockMapper.Setup(m => m.Map<Student>(studentVm)).Returns(student);
            _mockStudentRepository.Setup(repo => repo.UpdateStudentAsync(student)).ReturnsAsync(student);

            // Act
            var result = await _controller.PutStudent(studentId, studentVm);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<Student>(okResult.Value);
            Assert.Equal(studentId, returnValue.Id);
        }

        [Fact]
        public async Task BookHoliday_ReturnsNoContent()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var holidayVm = new HolidayVm { HolidayStart = new DateTimeOffset(new DateTime(2023, 6, 5)), HolidayEnd = new DateTimeOffset(new DateTime(2023, 6, 9)) };
            _mockStudentRepository.Setup(repo => repo.BookHolidayAsync(studentId, holidayVm.HolidayStart, holidayVm.HolidayEnd)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.BookHoliday(studentId, holidayVm);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteStudent_ReturnsNoContent()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var student = new Student { Id = studentId, FullName = "Kenneth Uche", Email = "info@meetkennethuche.com" };
            _mockStudentRepository.Setup(repo => repo.GetStudentByIdAsync(studentId)).ReturnsAsync(student);
            _mockStudentRepository.Setup(repo => repo.DeleteStudentAsync(studentId)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteStudent(studentId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteStudent_ReturnsNotFound()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            _mockStudentRepository.Setup(repo => repo.GetStudentByIdAsync(studentId)).ReturnsAsync((Student)null);

            // Act
            var result = await _controller.DeleteStudent(studentId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
 }


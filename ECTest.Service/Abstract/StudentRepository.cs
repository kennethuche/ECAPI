using ECTest.Service.Context;
using ECTest.Service.Interface;
using ECTest.Service.Models;
using ECTest.Service.UserException;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECTest.Service.Abstract
{
    public class StudentRepository : IStudentRepository
    {
        protected readonly ApplicationDBContext _dbContext;
        public StudentRepository(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Student> CreateStudentAsync(Student student)
        {
            // Validate courses
            if (student.Courses.Any())
            {
                foreach (var course in student.Courses)
                    course.NumberOfTutionWeek = CalculateTuitionWeeksForCourse(course);

                await ValidateCoursesAsync(student.Id, student.Courses);
            }
           


            _dbContext.Students.Add(student);
            await _dbContext.SaveChangesAsync();
            return student;
        }

        public async Task DeleteStudentAsync(Guid studentId)
        {
            var student = await _dbContext.Students
                .Include(s => s.Courses) // Include related courses
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student != null)
            {
                // Remove related courses
                _dbContext.Courses.RemoveRange(student.Courses);

                // Remove student
                _dbContext.Students.Remove(student);

                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<Student> GetStudentByIdAsync(Guid studentId)
        {
            var student = await _dbContext.Students
                .Include(s => s.Courses)
                .Include(s => s.Holidays)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
            {
                throw new KeyNotFoundException($"Student with ID {studentId} not found.");
            }

            return student;
        }
        public IQueryable<Student> GetStudents()
        {
            return _dbContext.Students
                .Include(s => s.Holidays)
                .Include(s => s.Courses)
                .AsQueryable();
        }

   
        public async Task<Student> UpdateStudentAsync(Student student)
        {

       

            // Find the existing student entity including related courses
            var existingStudent = await _dbContext.Students
                .Include(s => s.Courses)
                .FirstOrDefaultAsync(s => s.Id == student.Id);

            if (existingStudent == null)
            {
                throw new CustomValidationException("Student does not exist");
            }

            // Update properties of the existing student
            _dbContext.Entry(existingStudent).CurrentValues.SetValues(student);

            // Remove courses that are not in the updated student's list
            foreach (var existingCourse in existingStudent.Courses.ToList())
            {
                if (!student.Courses.Any(c => c.Id == existingCourse.Id))
                {
                    existingStudent.Courses.Remove(existingCourse);
                }
            }

            // Add new courses
            foreach (var course in student.Courses)
            {
                if (!existingStudent.Courses.Any(c => c.Id == course.Id))
                {
                    // If the course is not already associated with the student, add it
                   // existingStudent.Courses.Add(course);
                   _dbContext.Courses.Add(course);
                }
                else
                {
                    // If the course exists, update its properties
                    var existingCourse = existingStudent.Courses.First(c => c.Id == course.Id);
                    _dbContext.Entry(existingCourse).CurrentValues.SetValues(course);
                }
            }
            // Validate courses before making any changes
            await ValidateCoursesAsync(student.Id, student.Courses);
            await _dbContext.SaveChangesAsync();

            return existingStudent;



        }

        public async Task BookHolidayAsync(Guid studentId, DateTimeOffset holidayStart, DateTimeOffset holidayEnd)
        {
            if (holidayStart.DayOfWeek != DayOfWeek.Monday || holidayEnd.DayOfWeek != DayOfWeek.Friday)
            {
                throw new CustomValidationException("Holiday must start on a Monday and end on a Friday.");
            }

            var student = await GetStudentByIdAsync(studentId);
            if (student == null)
            {
                throw new CustomValidationException("Student does not exist");
            }

            var daysToExtend = (holidayEnd - holidayStart).Days + 1;
        
            var adjustedCourses = new List<Course>();

            var coursesToAdjust = student.Courses.OrderBy(c => c.StartDate).ToList();
            var endOfHolidayAdjustedCourse = holidayEnd;
            DateTimeOffset minAdjustedStartDate = DateTimeOffset.MaxValue;
            DateTimeOffset maxAdjustedEndDate = DateTimeOffset.MinValue;
            foreach (var course in coursesToAdjust)
            {
                if (course.EndDate >= holidayStart && course.StartDate <= holidayEnd)
                {
                    // Course intersects with holiday period, so adjust end date
                    course.StartDate = course.StartDate = course.StartDate < holidayStart ? course.StartDate : AdjustDateToStartOfWeek(endOfHolidayAdjustedCourse.AddDays(3));
                    course.EndDate = AdjustDateToEndOfWeek(course.EndDate.AddDays(daysToExtend));
                    endOfHolidayAdjustedCourse = course.EndDate;

                    minAdjustedStartDate = course.StartDate < minAdjustedStartDate ? course.StartDate : minAdjustedStartDate;
                    maxAdjustedEndDate = course.EndDate > maxAdjustedEndDate ? course.EndDate : maxAdjustedEndDate;



                    // Validate if course starts on Monday and ends on Friday after adjustment
                    if (course.StartDate.DayOfWeek != DayOfWeek.Monday || course.EndDate.DayOfWeek != DayOfWeek.Friday)
                    {
                        throw new CustomValidationException("Adjusted courses must start on a Monday and end on a Friday.");
                    }

                    adjustedCourses.Add(course);


                }
                else if(course.StartDate < holidayStart && course.EndDate < holidayEnd) // courses not affected by the Holiday
                {
                    adjustedCourses.Add(course);
                }
            }

            if(adjustedCourses.Any())
            coursesToAdjust = coursesToAdjust.Except(adjustedCourses).ToList();
            // Adjust subsequent courses to avoid overlap
            AdjustSubsequentCourses(coursesToAdjust, endOfHolidayAdjustedCourse);

            // Add holiday records to the database
           var holiday =
                  new Holiday
                  {
                      StartDate = holidayStart,
                      EndDate = holidayEnd,
                     StudentId = studentId,
                  };

            _dbContext.Holidays.Add(holiday);
            

            await _dbContext.SaveChangesAsync();
        }


        private void AdjustSubsequentCourses(List<Course> courses, DateTimeOffset startAfter)
        {
            for (int i = 0; i < courses.Count; i++)
            {
                var course = courses[i];
                if (course.StartDate < startAfter)
                {
                    var duration = (course.EndDate - course.StartDate).Days;
                    var newStartDate = AdjustDateToStartOfWeek(startAfter.AddDays(3)); // Move to the next Monday
                    var newEndDate = AdjustDateToEndOfWeek(newStartDate.AddDays(duration));

                    course.StartDate = newStartDate;
                    course.EndDate = newEndDate;

                    startAfter = newEndDate;
                }
            }
        }

        private DateTimeOffset AdjustDateToStartOfWeek(DateTimeOffset date)
        {
            while (date.DayOfWeek != DayOfWeek.Monday)
            {
                date = date.AddDays(-1);
            }
            return date;
        }

        private DateTimeOffset AdjustDateToEndOfWeek(DateTimeOffset date)
        {
            while (date.DayOfWeek != DayOfWeek.Friday)
            {
                date = date.AddDays(1);
            }
            return date;
        }

        private int CalculateTuitionWeeksForCourse(Course course)
        {
            DateTimeOffset currentStart = course.StartDate;
            DateTimeOffset courseEnd = course.EndDate;
            int tuitionWeeks = 0;

            while (currentStart <= courseEnd)
            {
                // Check if the current week is a full tuition week (Monday to Friday)
                if (currentStart.DayOfWeek == DayOfWeek.Monday)
                {
                    DateTimeOffset weekEnd = currentStart.AddDays(4); // End of the current week (Friday)
                    if (weekEnd <= courseEnd && weekEnd.DayOfWeek == DayOfWeek.Friday)
                    {
                        tuitionWeeks++;
                        currentStart = currentStart.AddDays(7); // Move to the next Monday
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    // Move to the next Monday
                    currentStart = currentStart.AddDays((DayOfWeek.Monday - currentStart.DayOfWeek + 7) % 7);
                }
            }

            return tuitionWeeks;
        }

        private async Task ValidateCoursesAsync(Guid studentId, IEnumerable<Course> courses)
        {
            foreach (var course in courses)
            {
                // Validate if course starts on Monday and ends on Friday
                if (course.StartDate.DayOfWeek != DayOfWeek.Monday || course.EndDate.DayOfWeek != DayOfWeek.Friday)
                {
                    throw new CustomValidationException("Courses must start on a Monday and end on a Friday.");
                }

                // Check for overlapping courses
                var overlappingCourse = await _dbContext.Courses
                    .AnyAsync(c => c.StudentId == studentId &&
                                   ((course.StartDate >= c.StartDate && course.StartDate <= c.EndDate) ||
                                    (course.EndDate >= c.StartDate && course.EndDate <= c.EndDate) ||
                                    (c.StartDate >= course.StartDate && c.StartDate <= course.EndDate) ||
                                    (c.EndDate >= course.StartDate && c.EndDate <= course.EndDate)));

                if (overlappingCourse)
                {
                    throw new CustomValidationException("Overlapping courses are not allowed.");
                }
            }
        }
    }
}

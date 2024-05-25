using ECTest.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECTest.Service.Interface
{
    public interface IStudentRepository
    {
        Task<Student> CreateStudentAsync(Student student);
        IQueryable<Student> GetStudents();

        Task DeleteStudentAsync(Guid studentId);
        Task<Student> GetStudentByIdAsync(Guid StudentId);
        Task<Student> UpdateStudentAsync(Student student);

        Task BookHolidayAsync(Guid studentId, DateTimeOffset holidayStart, DateTimeOffset holidayEnd);
    }
}

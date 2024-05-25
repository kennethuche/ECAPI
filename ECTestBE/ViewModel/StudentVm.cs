using ECTest.Service.Models;
using System.ComponentModel.DataAnnotations;

namespace ECTestBE.ViewModel
{
    public class StudentVm
    {
        public string FullName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }
        public  IEnumerable<CourseVm> Courses { get; set; }
    }


}

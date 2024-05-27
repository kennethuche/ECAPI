using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECTest.Service.Models
{
    public class Course
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string CourseName { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }

        public int NumberOfTutionWeek { get; set; }
        [ForeignKey("Student")]
        public Guid StudentId { get; set; }
        public  virtual Student Student { get; set; }
    }
}

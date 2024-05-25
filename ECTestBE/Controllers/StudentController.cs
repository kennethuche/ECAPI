using AutoMapper;
using ECTest.Service.Interface;
using ECTest.Service.Models;
using ECTestBE.ViewModel;
using Microsoft.AspNetCore.Mvc;


namespace ECTestBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IStudentRepository _studentRepository;
        private readonly ILogger<StudentController> _logger;
        private readonly IMapper _mapper;

        public StudentController(ILogger<StudentController> logger, IMapper mapper, IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
            _logger = logger;
            _mapper = mapper;
        }

        // GET: api/Student
        [HttpGet]
        public ActionResult<IEnumerable<Student>> GetStudents()
        {
            var students =  _studentRepository.GetStudents();

            return Ok(students);
        }

        // GET: api/Student/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudent(Guid id)
        {
           
                var student = await _studentRepository.GetStudentByIdAsync(id);

            return Ok(student);


           
        }

        // POST: api/Student
        [HttpPost]
        public async Task<ActionResult<Student>> PostStudent(StudentVm req)
        {
            var student = _mapper.Map<Student>(req);

            if (student.Courses.Any())
            {
                foreach (var course in student.Courses)
                    course.StudentId = student.Id;
            }
        

            var createdStudent = await _studentRepository.CreateStudentAsync(student);
             return Created("Success", createdStudent.Id);

        }


        // PUT: api/Student/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStudent(Guid id, StudentVm req)
        {

            // Check if the student exists
            if (!await StudentExists(id))
                return NotFound();
            

            // Map the view model to the entity model
            var student = _mapper.Map<Student>(req);
            student.Id = id;

            // Ensure each course has the correct StudentId
            if (student.Courses != null)
                foreach (var course in student.Courses)
                 course.StudentId = student.Id;
            

            // Update the student in the repository
            var updatedStudent = await _studentRepository.UpdateStudentAsync(student);
            return Ok(updatedStudent);

        }




        [HttpPost("{id}/holiday")]
        public async Task<IActionResult> BookHoliday(Guid id, [FromBody] HolidayVm holidayVm)
        {
            await _studentRepository.BookHolidayAsync(id, holidayVm.HolidayStart, holidayVm.HolidayEnd);
            return NoContent();
        }

        // DELETE: api/Student/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(Guid id)
        {

            if (!await StudentExists(id))
                return NotFound();
            

            await _studentRepository.DeleteStudentAsync(id);
                return NoContent();
           
         
        }

        private async Task<bool> StudentExists(Guid id)
        {
            var student = await _studentRepository.GetStudentByIdAsync(id);
            return student != null;
        }

    }
    
}

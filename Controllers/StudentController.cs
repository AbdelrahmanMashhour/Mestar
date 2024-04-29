using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RepositoryPatternWithUOW.Core.Interfaces;

namespace Mestar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Student")]
    public class StudentController(IUnitOfWork unitOfWork) : ControllerBase
    {
        [HttpGet("StudentProfile")]
        public async Task<IActionResult> GetStudentProfile(int id)
        {
            var profile=await unitOfWork.UserRepository.GetProfileData(id);
            return Ok(profile);
        }

        [HttpGet("AllCoursesForStudent")]
        public async Task<IActionResult> AllCoursesForStudent(int studentId)
        {

            var courses = await unitOfWork.CourseRepository.AllCoursesAsync(studentId);
            return Ok(courses);

        }
    }
}

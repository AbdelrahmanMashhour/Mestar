using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RepositoryPatternWithUOW.Core.DTOs.AssignmentDtos;
using RepositoryPatternWithUOW.Core.DTOs.CourseDTOs;
using RepositoryPatternWithUOW.Core.Enums;
using RepositoryPatternWithUOW.Core.Interfaces;
using RepositoryPatternWithUOW.Core.Models;
using RepositoryPatternWithUOW.EfCore.Mapper;
using static System.Net.Mime.MediaTypeNames;

namespace Mestar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController(IUnitOfWork unitOfWork,Mapper mapper) : ControllerBase
    {

        [HttpGet("AllCoursesInSameStage")]
        public async Task<IActionResult> AllCoursesForStage(Stages stage)
        {

            var courses = await unitOfWork.CourseRepository.FindAllAsync(x => x.CoursStage == stage);
            //$"{Request.Scheme}://{Request.Host}/{Image-Name}"
            foreach (var course in courses)
            {
                if (course.ProfileUrl!=null)
                {
                    course.ProfileUrl = $"{Request.Scheme}://{Request.Host}{course.ProfileUrl}";
                }
            }
            return Ok(courses);

        }
        [HttpGet("AllUnitesByCourseId/{id}")]
        //[Authorize]
        public async Task<IActionResult> AllUnitesByCourseIdForAdmin(int id)
        {
            var unites = await unitOfWork.UniteRepository.AllUnitesByCourseId(id,1 );
            if (unites is null)
            {
                return NotFound();
            }
            return Ok(unites);
        }

        [HttpGet("AllUnitesByCourseIdForStudent/{id}")]
        public async Task<IActionResult> AllUnitesByCourseIdForStudent(int id, int studentId)
        {
            var unites = await unitOfWork.UniteRepository.AllUnitesByCourseId(id, studentId);
            if (unites is null)
            {
                return NotFound();
            }
            return Ok(unites);
        }



        [HttpGet("UniteById/{UniteId}")]
        public async Task<IActionResult> UniteById(int UniteId)
        {
            var unit=await unitOfWork.UniteRepository.FindAsync(x=>x.UnitId == UniteId);
            if (unit == null)
                return BadRequest("Your Id is InCorrect...");

            unit.SkillUrl = unit.SkillUrl is null ? null : HttpContext.Request.Scheme + "://" + HttpContext.Request.Host + unit.SkillUrl;


            unit.SkillPdfUrl = unit.SkillPdfUrl is null ? null : HttpContext.Request.Scheme + "://" + HttpContext.Request.Host + unit.SkillPdfUrl;


            unit.TranslationPdfUrl = unit.TranslationPdfUrl is null ? null : HttpContext.Request.Scheme + "://" + HttpContext.Request.Host + unit.TranslationPdfUrl;


            unit.TranslationUrl = unit.TranslationUrl is null ? null : HttpContext.Request.Scheme + "://" + HttpContext.Request.Host + unit.TranslationUrl;


            unit.ExamUrl = unit.ExamUrl is null ? null : HttpContext.Request.Scheme + "://" + HttpContext.Request.Host + unit.ExamUrl;

            unit.VocablaryPdfUrl = unit.VocablaryPdfUrl is null ? null : HttpContext.Request.Scheme + "://" + HttpContext.Request.Host + unit.VocablaryPdfUrl;

            unit.VocablaryUrl = unit.VocablaryUrl is null ? null : HttpContext.Request.Scheme + "://" + HttpContext.Request.Host + unit.VocablaryUrl;


            unit.StoryPdfUrl = unit.StoryPdfUrl is null ? null : HttpContext.Request.Scheme + "://" + HttpContext.Request.Host + unit.StoryPdfUrl;



            unit.StoryUrl = unit.StoryUrl is null ? null : HttpContext.Request.Scheme + "://" + HttpContext.Request.Host + unit.StoryUrl;




                foreach (var nested in unit.Assignment)
                {
                    nested.AssFiles = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host + nested.AssFiles;
                }




            return Ok(unit);
        }
        //[HttpGet("AllStudentInCoursByCoursId")]
        //public async Task<IActionResult> AllStudentInCoursByCourseId(int CourseId)
        //{
        //    var result = await unitOfWork.StudentCourseRepository.AllStudentInCoursByCourseId(CourseId);
        //    if (result!=null)
        //    {
        //        var retriveData = new List<RetriveStudentInCoursDto>();
        //        foreach (var item in result)
        //        {
        //            retriveData.Add(new RetriveStudentInCoursDto { Email = item.Email, Id = item.UserId, StudentName = item.FirstName + " " + item.LastName });
                    

        //        }
        //        return Ok(retriveData);

        //    }

        //    return Ok(result);



        //}


        [HttpGet("GetGradeOfExam")]
        public async Task<IActionResult> GetGradeOfExam([FromBody]ExamDataDto dto)
        {

            var result= await unitOfWork.SolutionRepository.GetGradeOfExam(dto);
            if(result==-1)
            {
                return BadRequest("AssignmentId isn't correct");
            }
            if(result==-2)
            {
                return BadRequest("You Don't Submite");

            }
            if (result==null)
            {
                return BadRequest("Exam not checked");
            }
            return Ok(result);
        }

        [HttpGet("AssignmentsOfStudentsToAddGrade")]
        public async Task<IActionResult> AllAssignmentsOfStudents()
        {

            var result = await unitOfWork.SolutionRepository.GetSolutionsData();
            return Ok(result);

        }

        //Admine
        [HttpGet("AllCourses")]
        public async Task<IActionResult> AllCoures()
        {
            var courses = await unitOfWork.CourseRepository.AllCoursesAsync();
            return Ok(courses);
        }
        [HttpPost("AddCourse")]
        public async Task<IActionResult> AddCourse([FromForm]AddCourseDto dto)
        {
            try
            {
                  var course = new Course()
                    {
                        CourseName = dto.CourseName,
                        CourseDescription = dto.CourseDescription,
                        CoursePrice = dto.CoursePrice,
                        CoursStage = dto.CoursStage,
                        TotoalHoure = dto.TotoalHoure,
                        AdminId = 1
                    };

                //$"{Request.Scheme}://{Request.Host}/{Image-Name}"

                if (dto.Profile != null && dto.Profile.Length != 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.Profile.FileName);
                    var filePath = Path.Combine("wwwroot/images", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await dto.Profile.CopyToAsync(stream);
                    }

                        
                    course.ProfileUrl = Url.Content("~/images/" + fileName);


                }
                await unitOfWork.CourseRepository.AddAsync(course);
                await unitOfWork.SaveChangesAsync();
                return Ok(dto);

            }
            catch
            {
                return BadRequest("Can't Add");
            }

        }



        [HttpPost("AddUniteToCourse")]
        public async Task<IActionResult> AddUniteToCours([FromForm] UnitDto unitDto)
        {
            var course = await unitOfWork.CourseRepository.FindAllAsync(x => x.CourseId == unitDto.CourseId);
            if (course is null)
            {
                return BadRequest("You Don't Have Course With This Id");
            }
            try
            {
               
                await unitOfWork.UniteRepository.AddUnitAsync(unitDto);
                await unitOfWork.SaveChangesAsync();
                var uniteId = await unitOfWork.UniteRepository.LastUnitId();
                return Ok(uniteId);
            }
            catch 
            {
                return BadRequest("Can't Add Unit");
            }
        }


        [HttpPost("AddAssignment")]
        public async Task<IActionResult> AddAssignment(AssignmentDto assignmentDto)
        {

            var result = await unitOfWork.AssignmentRepository.AddAssignmentAsync(assignmentDto);
            if (result)
            {
                try
                {
                    await unitOfWork.SaveChangesAsync();
                    return Created();
                }
                catch 
                {
                    return BadRequest("Can't Add Assignment");
                }
            }
            return BadRequest("Can't Add Assignment");
        }

        [HttpPost("UploadSolution")]
        public async Task<IActionResult> UploadSolution(SolutionDto dto)
        {
            var result = await unitOfWork.SolutionRepository.UploadSolution(dto);
            if (result)
            {
                try
                {
                    await unitOfWork.SaveChangesAsync();
                    return Created();
                }
                catch
                {
                    return BadRequest("Can't Upload");
                }
            }
          
           return BadRequest("Can't Upload");
        }

        [HttpPost("GiveGrade")]
        public async Task<IActionResult> GiveGrade(GradeDto dto)

        {
            var result = await unitOfWork.SolutionRepository.GiveGradeToStudent(dto);
            if (result)
            {
                try
                {
                    await unitOfWork.SaveChangesAsync();
                    return Created();
                }
                catch
                {
                    return BadRequest("Can't Give Grade To This Student");
                }
            }

            return BadRequest("Can't Give Grade To This Student");
        }


        [HttpDelete("RemoveAllCoursesInStage")]
        public async Task<IActionResult> DeleteAllCoursesInStage(Stages stage)
        {
            var courses= await unitOfWork.CourseRepository.FindAllAsync(x => x.CoursStage == stage);
            if (courses is null)
            {
                return BadRequest("This Stage Don't Have Any Courses");
            }
            try
            {
                unitOfWork.CourseRepository.DeleteAll(courses);
                await unitOfWork.SaveChangesAsync();
                return Ok(courses);
            }
            catch 
            {
                return BadRequest("Can't Remove");

            }


        }

        //[HttpDelete("RemoveUnitById/{id}")]
        //public async Task<IActionResult> RemoveUniteById(int id)
        //{
        //    try
        //    {
        //        bool result = await unitOfWork.UniteRepository.DeleteUniteById(id);
        //        if (!result)
        //            return BadRequest("Can't Remove");
        //        return Ok();
        //    }
        //    catch 
        //    {

        //        return BadRequest("Can't Remove");
        //    }
        //}

        [HttpDelete("CourseById")]
        public async Task<IActionResult> DeleteCourseByItsId(int id)
        {
            var result = await unitOfWork.CourseRepository.DeleteCourseById(id);
            if (result)
                return Ok();
            return BadRequest("Can't Delete");
        }
    }
}

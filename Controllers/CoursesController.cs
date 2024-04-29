using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
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
        //[Authorize(Roles = "Student")]
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

        //[Authorize(Roles = "Admin")]
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


        //[Authorize(Roles = "Student")]
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


        //[Authorize]
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



        //[Authorize]
        [HttpGet("GetGradeOfExam")]
        public async Task<IActionResult> GetGradeOfExam(int id)
        {

            var result= await unitOfWork.SolutionRepository.GetAllResolvedExam(id);
            
            return Ok(result);
        }


        //[Authorize(Roles ="Admin")]
        [HttpGet("AssignmentsOfStudentsToAddGrade")]
        public async Task<IActionResult> AllAssignmentsOfStudents()
        {

            var result = await unitOfWork.SolutionRepository.GetSolutionsData();
            return Ok(result);

        }

        //Admine

        //[Authorize]
        [HttpGet("AllCourses")]
        public async Task<IActionResult> AllCoures()
        {
            var courses = await unitOfWork.CourseRepository.AllCoursesAsync();
            return Ok(courses);
        }

        //[Authorize(Roles = "Admin")]
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
                var coursId = await unitOfWork.CourseRepository.LastCourseId();
                var obj = new { coursId=coursId,dto=dto };
                return Ok(obj);

            }
            catch
            {
                return BadRequest("Can't Add");
            }

        }


        //[Authorize(Roles = "Admin")]
        [HttpPost("AddUniteToCourse")]
        public async Task<IActionResult> AddUniteToCours([FromForm] UnitDto unitDto)
        {

            if (!await unitOfWork.CourseRepository.IsExist(x => x.CourseId == unitDto.CourseId))
            {
                return BadRequest("You Don't Have Course With This Id");
            }

            if (!unitDto.Vocablary.ContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only video files are allowed");
            try
            {

                var uniteId = await unitOfWork.UniteRepository.AddUnitAsync(unitDto);

                return Ok(uniteId);

            }
            catch
            {
                return BadRequest("Can't Add Unit");
            }
        }

       // [Authorize(Roles = "Admin")]
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


        //[Authorize(Roles = "Student")]
        [HttpPost("UploadSolution")]
        public async Task<IActionResult> UploadSolution([FromForm]SolutionDto dto)
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

        //[Authorize(Roles = "Admin")]
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

        //[Authorize(Roles = "Admin")]

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


        //[Authorize(Roles = "Admin")]
        [HttpDelete("CourseById")]
        public async Task<IActionResult> DeleteCourseByItsId(int id)
        {
            var result = await unitOfWork.CourseRepository.DeleteCourseById(id);
            if (result)
                return Ok();
            return BadRequest("Can't Delete");
        }


       // [Authorize(Roles = "Admin")]
        [HttpPut("UpdateUniteById/{id}")]
        public async Task<IActionResult> UpdateUniteById(UnitDto dto,int id)
        {
            var result = await unitOfWork.UniteRepository.UpdateUniteAsync(dto,id);
            await unitOfWork.SaveChangesAsync();
            return result ? Ok() : BadRequest();
        }


        //[Authorize(Roles = "Admin")]
        [HttpPatch("UpdateCourseById/{id}")]
        public async Task<IActionResult> UpdateCourseData(JsonPatchDocument<Course> course,[FromRoute]int id)
        {
            var result = await unitOfWork.CourseRepository.UpdateCourse(course, id);

            return result ? Ok() : BadRequest( "Can't Update");
        }




        [HttpGet("IsPayOrNot")]
        public async Task<IActionResult> StudentPayOrNot(int studentId,int courseId)
        {
            

            return await unitOfWork.StudentCourseRepository.IsPayOrNot(studentId, courseId)?Ok():NotFound();
            
            
        }

    }
}

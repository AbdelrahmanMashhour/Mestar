using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RepositoryPatternWithUOW.Core.DTOs.PayProcess;
using RepositoryPatternWithUOW.Core.Interfaces;

namespace Mestar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class PaymentController(IUnitOfWork unitOfWork) : ControllerBase
    {
        [HttpPost("AddStudentToCourse")]
        public async Task<IActionResult> AddStudentToCourse(PayInputDto dto)
        {
            var result= await unitOfWork.UniteRepository.AddStudentToCourse(dto);
            if(result!= "Sucsess Process")
            {
                return BadRequest(result);
            }
            try
            {
                await unitOfWork.SaveChangesAsync();
                return Ok(result);
            }
            catch
            {
                return BadRequest("Sorry Failed in process");
            }

        }



        [HttpDelete("DeleteStudentFromCourse")]
        public async Task<IActionResult> DeleteStudentFromCourse(PayInputDto dto)
        {
            var result = await unitOfWork.UniteRepository.DeleteStudentToCourse(dto);
            if (result != "Sucsess Process")
            {
                return BadRequest(result);
            }
            return Ok(result);


        }
    }
}

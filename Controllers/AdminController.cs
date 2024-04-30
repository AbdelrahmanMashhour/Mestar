using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepositoryPatternWithUOW.Core.Interfaces;

namespace Mestar.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController(IUnitOfWork unitOfWork):ControllerBase
    {
        [HttpGet("GetStudents")]
        
        public async Task<IActionResult> GetAllStudents(bool bloked)
        {
            var students=await unitOfWork.UserRepository.GetAllStudents(bloked);
            return Ok(students);
        }

        [HttpPost("AddToBlackList/{id}")]
        public async Task<IActionResult> AddToBlackList(int id)
        {
            var result = await unitOfWork.UserRepository.AddToBlackList(id);
            if (result)
            {
                return Ok();
            }
            return BadRequest("Can't Add");
        } 

        
        [HttpPost("RemoveFromBlackList/{id}")]
        
        public async Task<IActionResult> RemoveFromBlackList(int id)
        {
            var result = await unitOfWork.UserRepository.RemoveFromBlackList(id);
            if (result)
            {

                return Ok();
            }
            return BadRequest("Can't Remove");
        }

   


    }
}

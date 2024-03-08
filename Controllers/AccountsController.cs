using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RepositoryPatternWithUOW.Core.Dto;
using RepositoryPatternWithUOW.Core.DTOs;
using RepositoryPatternWithUOW.Core.Interfaces;

namespace Mestar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        IUnitOfWork unitOfWork;

        public AccountsController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        [HttpPost("SignUp")]
        public async Task<IActionResult> SignUp(SignUpDto signUpDto)
        {
            var result = await unitOfWork.UserRepository.SignUpAsync(signUpDto);
            if (!result)
                return BadRequest();
            await unitOfWork.SaveChangesAsync();

            return Ok();
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var result = await unitOfWork.UserRepository.LoginAsync(loginDto);

            if (result.Success)
            {
                await unitOfWork.SaveChangesAsync();
                return Ok(result);
            }
            return NotFound();
        }
        [HttpPost("SendCode")]
        public async Task<IActionResult> SendConfirmationCode(SendCodeDto sendCodeDto)
        {
            var result = await unitOfWork.UserRepository.SendVerficationCode(sendCodeDto.Email, sendCodeDto.Reset is null ? false : true);
            if (!result)
                return NotFound();
            await unitOfWork.SaveChangesAsync();
            return Ok();

        }
        [HttpPost("ValidateEmailVerificationCode")]

        public async Task<IActionResult> ValidateConfirmationCode(ValidationCodeDto VCD)
        {
            var result = await unitOfWork.UserRepository.ValidateCode(VCD.Email, VCD.Code);
            await unitOfWork.SaveChangesAsync();
            if (!result)
                return Forbid();
            return Ok();

        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using RepositoryPatternWithUOW.Core.Dto;
using RepositoryPatternWithUOW.Core.DTOs;
using RepositoryPatternWithUOW.Core.Interfaces;
using RepositoryPatternWithUOW.Core.Models;
using RepositoryPatternWithUOW.EF;
using System.IdentityModel.Tokens.Jwt;

namespace Mestar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;

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

            if (result.Success&&(result.EmailConfirmed==false))
              return BadRequest("Your Email Not Conformed");


            if (result.Success&&result.EmailConfirmed)
            {
                await unitOfWork.SaveChangesAsync();

                SetCookie("accessToken", result.Jwt, (DateTime)result.ExpirationOfJwt);

                SetCookie("refreshToken", result.RefreshToken, (DateTime)result.ExpirationOfRefreshToken);

                //result.RefreshToken = null;
                //result.ExpirationOfRefreshToken = null;
                //result.ExpirationOfJwt = null;


                return Ok(result);
            }
            return NotFound();
        }
        private void SetCookie(string name, string value,DateTime expiresOn)
        {

            var cookieOptions = new CookieOptions();
           // cookieOptions.Secure = true;
           cookieOptions.HttpOnly = true;
           cookieOptions.Expires = expiresOn.ToLocalTime();
            
            //cookieOptions.SameSite = SameSiteMode.None;//not exit in wwwroot
            //cookieOptions.SameSite = SameSiteMode.Strict;//wwwroot
            

            Response.Cookies.Append(name, value, cookieOptions);

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
        //[Authorize]
        [HttpPost("ValidateResetPasswordCode")]
        public async Task<IActionResult> ValidateResetPasswordCode(ValidationCodeDto VCD)
        {
            var result = await unitOfWork.UserRepository.ValidateCode(VCD.Email, VCD.Code, true);
            await unitOfWork.SaveChangesAsync();
            if (!result)
                return Forbid();
            return Ok();

        }


        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            var result = await unitOfWork.UserRepository.ResetPassword(resetPasswordDto);
            if (!result)
                return BadRequest();
            await unitOfWork.SaveChangesAsync();
            return Ok();
        }


        [Authorize]
        [HttpPost("UpdatePassword")]
        public async Task<IActionResult> UpdatePassword(UpdatePasswordDto updatePasswordDto)
        {
            var email = ExtractEmail();
            var result = await unitOfWork.UserRepository.UpdatePasswordAsync(email, updatePasswordDto);
            if (!result)
            {
                return BadRequest();
            }
            await unitOfWork.SaveChangesAsync();
            return Ok();
        }


        [Authorize(Roles = "Student")]
        //[Authorize]
        [HttpPatch("UpdateInsensitveData")]
        public async Task<IActionResult> UpdateInsensitiveData([FromBody] JsonPatchDocument<User> patchDocument)
        {
            string? email = ExtractEmail();
            if (email is null)
                return BadRequest();

            var result = await unitOfWork.UserRepository.UpdateInsensitiveData(patchDocument, email);
            if (!result)
                return BadRequest();
            try
            {
            await unitOfWork.SaveChangesAsync();
            }
            catch
            {
                return BadRequest();
            }
            return Ok();
        }


        //[Authorize(Roles = "Student")]
        [Authorize]
        [HttpPost("UpdatePicture")]
        public async Task<IActionResult> UpdateProfilePicture([FromForm] UpdateProfilePictureDto newPicture)
        {

            var email = ExtractEmail();

            var result = await unitOfWork.UserRepository.UpdateProfilePicture(new() { NewPicture = newPicture.NewPicutre, Email = email });
            if (!result) { return BadRequest(); }
            await unitOfWork.SaveChangesAsync();
            return Ok();



        }

        private string ExtractEmail()
        {
            // Retrieve the JWT token from the request headers
            var test = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            var token=test?.Replace("Bearer ", "");

            // Parse the JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            // Retrieve the email claim from the token
            var email = jwtToken?.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

            if (email is null)
            {
                return string.Empty;
            }
            return email;
        }


        [HttpPost("UpdateTokens")]
        public async Task<IActionResult> UpdateTokens(UpdateTokensDto updateTokenDto)
        {
            try
            {
                var result = await unitOfWork.UserRepository.UpdateTokens(updateTokenDto);
                await unitOfWork.SaveChangesAsync();
                if (!result.Success)
                {
                    return Unauthorized();
                }

                return Ok(result);
            }
            catch
            {
                return NotFound();
            }

        }

        [Authorize]
        [HttpDelete("SignOut")]
        public async Task<IActionResult> SignOut([FromBody] string refreshToken)
        {
            var email = ExtractEmail();

            var result = await unitOfWork.UserRepository.SignOut(refreshToken, email);
            if (result)
                await unitOfWork.SaveChangesAsync();

            return Ok();

        }

        
    }
}

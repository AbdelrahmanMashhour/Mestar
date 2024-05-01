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
using System.Security.Claims;

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

                SetCookie("accessToken", result.Jwt, (DateTime)result.ExpirationOfJwt,true);

                SetCookie("refreshToken", result.RefreshToken, (DateTime)result.ExpirationOfRefreshToken,true);

                SetCookie("firstName", result.FirstName, (DateTime)result.ExpirationOfRefreshToken);
                SetCookie("lastName", result.LastName, (DateTime)result.ExpirationOfRefreshToken);

                SetCookie("role", result.Role, (DateTime)result.ExpirationOfRefreshToken);

                SetCookie("email", result.Email, (DateTime)result.ExpirationOfRefreshToken);

                SetCookie("id", result.Id.ToString(), (DateTime)result.ExpirationOfRefreshToken);



                //result.RefreshToken = null;
                //result.ExpirationOfRefreshToken = null;
                //result.ExpirationOfJwt = null;


                return Ok();
            }
            return NotFound();
        }
        private void SetCookie(string name, string value,DateTime expiresOn,bool httpOnlyValue=false)
        {

            var cookieOptions = new CookieOptions();
            cookieOptions.Secure = true;//http
            cookieOptions.HttpOnly = httpOnlyValue;
            cookieOptions.Expires = expiresOn;

            cookieOptions.SameSite = SameSiteMode.Strict;
            //cookieOptions.SameSite = SameSiteMode.None;//wwwroot


            Response.Cookies.Append(name, value, cookieOptions);

        }


        [HttpPost("SendCode")]
        public async Task<IActionResult> SendConfirmationCode(SendCodeDto sendCodeDto)
        {
            var result = await unitOfWork.UserRepository.SendVerficationCode(sendCodeDto.Email, sendCodeDto.Reset is null ? false : true);
            if (!result)
                return NotFound();
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
            if (!HttpContext.Request.Cookies.TryGetValue("accessToken",out string valu))
            {
                return string.Empty;
            }
                
            //    .Headers["Authorization"].FirstOrDefault();
            //var token=test?.Replace("Bearer ", "");

            // Parse the JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(valu) as JwtSecurityToken;

            // Retrieve the email claim from the token
            var email = jwtToken?.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

            if (email is null)
            {
                return string.Empty;
            }
            return email;
        }

        private int ExtractId()
        {
            // Retrieve the JWT token from the request headers
            if (!HttpContext.Request.Cookies.TryGetValue("accessToken", out string valu))
            {
                return -1;
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(valu) as JwtSecurityToken;


            var Id = jwtToken?.Payload[ClaimTypes.NameIdentifier] as int?;
            if (Id is null)
            {
                return -1;
            }
            return (int)Id;
        }


        [HttpPost("UpdateTokens")]
        public async Task<IActionResult> UpdateTokens(string email)
        {
            try
            {
                if (!Request.Cookies.TryGetValue("refreshToken", out string? refreshToken))
                {
                    return Unauthorized();
                }
                
                
                var result = await unitOfWork.UserRepository.UpdateTokens(new UpdateTokensDto { Email=email,RefreshToken=refreshToken});
                await unitOfWork.SaveChangesAsync();

                if (!result.Success)
                {
                    return Unauthorized();
                }


                SetCookie("accessToken", result.Jwt, (DateTime)result.ExpirationOfJwt, true);

                SetCookie("refreshToken", result.RefreshToken, (DateTime)result.ExpirationOfRefreshToken, true);

                return Ok();
            }
            catch
            {
                return Unauthorized();
            }

        }

        [Authorize]
        [HttpDelete("SignOut")]
        public async Task<IActionResult> SignOut()
        {
            if (!Request.Cookies.TryGetValue("refreshToken",out string? value))
            {
                return Unauthorized();
            }
            var email = ExtractEmail();
            var result = await unitOfWork.UserRepository.SignOut(value, email);
            if (result)
            {
                //await unitOfWork.SaveChangesAsync();
            SetCookie("accessToken", "", DateTime.Now.AddDays(-1));
            SetCookie("refreshToken", "", DateTime.Now.AddDays(-1));
                return Ok();
            }

            return BadRequest("Can't Get Refresh_Token");


        }

        
        
    }
}

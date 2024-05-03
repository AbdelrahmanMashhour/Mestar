using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using RepositoryPatternWithUOW.Core.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Mestar
{
    public class FileMiddleWare(RequestDelegate next)
    {

       public async Task InvokeAsync(HttpContext context, IConfiguration configuration, IWebHostEnvironment webHostEnvironment,IUnitOfWork unitOfWork)
        {
            var ext = Path.GetExtension(context.Request.Path.Value);
             if (ext == ".txt")
            {
                
                
                    context.Response.ContentType = "text/plain; charset=utf-8";
                    var path = Path.Combine(webHostEnvironment.WebRootPath, context.Request.Path.Value.TrimStart('/'));
                    await context.Response.SendFileAsync(path);


                return;
            }

            var finalPathArr = context.Request.Path.Value.Split("/");
            var finalPath= finalPathArr[finalPathArr.Length-1];
            if (finalPath.StartsWith("video"))
            {
                

            if (context.Request.Query["videoID"].IsNullOrEmpty()||!decimal.TryParse(context.Request.Query["videoID"], out decimal cacheBustingVal))
            {
                context.Response.StatusCode = 401;
                return;
            }
                if (context.Request.Headers.TryGetValue("referer", out StringValues s))
                {
                 



                    if (context.Request.Cookies.TryGetValue("accessToken", out string? val))
                    {

               
                        var tokenHandler = new JwtSecurityTokenHandler();

                        var tvr = await tokenHandler.ValidateTokenAsync(val, new TokenValidationParameters()
                        {
                            ValidateAudience = true,
                            ValidateIssuer = true,
                            ValidateLifetime = true,
                            ValidAudience = configuration["JWT:Audience"],
                            ValidIssuer = configuration["JWT:Issuer"],
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:SecretKey"])),
                            RoleClaimType = ClaimTypes.Role,

                        }); ;
                        if (tvr.IsValid)
                        {
                            var payload = tokenHandler.ReadJwtToken(val).Payload;
                            var id = int.Parse(payload[ClaimTypes.NameIdentifier].ToString()!);
                            var connectionId=await unitOfWork.UserConnection.FindAsync(x=>x.StudentId== id);    
                            if (connectionId is not null && !connectionId.RequestedToVideo)
                            {
                                connectionId.RequestedToVideo = true;
                               


                                var connection = unitOfWork.UserConnection;
                                await unitOfWork.SaveChangesAsync();
                                await next(context);
                              
                            }
                            else
                            {
                                context.Response.StatusCode = 401;
                                return;
                            }
                        }
                        else
                        {
                            context.Response.StatusCode = 401;
                            return;
                        }

                    }
                    else
                        context.Response.StatusCode = 401;
                }
                else
                {
                    context.Response.StatusCode = 401;
                    return;  

                }

            }
            else
            await next(context);
          


        }
    }
}

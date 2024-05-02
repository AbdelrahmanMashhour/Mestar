using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Mestar
{
    public class FileMiddleWare(RequestDelegate next)
    {

       public async Task InvokeAsync(HttpContext context, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
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
            if(finalPath.StartsWith("video"))
            {
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
                            await next(context);

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

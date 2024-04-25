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
            if (context.Request.Path.StartsWithSegments("/Videos")&& !context.Request.Path.StartsWithSegments("/Videos/Pdfs"))
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
                        {
                            //if(tvr.Claims.Any(x=>x.Key==ClaimTypes.Role&&x.Value=="student"))
                            await next(context);
                        }
                        else
                        {
                            context.Response.StatusCode = 401;



                        }



                    }
                    else
                    {
                        context.Response.StatusCode = 401;
                    }
                }
                else
                {
                    context.Response.StatusCode = 401;
                }

            }
            else if (context.Request.Path.Value.Contains("Txts"))
            {
                context.Response.ContentType = "text/plain; charset=utf-8";
                var path = Path.Combine(webHostEnvironment.WebRootPath, context.Request.Path.Value.TrimStart('/'));
                await context.Response.SendFileAsync(path);

            }
            else
            {
                await next(context);
            }
        }
    }
}

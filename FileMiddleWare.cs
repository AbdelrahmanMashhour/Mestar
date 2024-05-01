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
            await next(context);

            
            //if (context.Response.ContentType!="text/javascript"&&
            //    context.Response.ContentType!="text/css" && 
            //    context.Request.Path!="/index.html"&&
            //    context.Response.StatusCode!=404&&
            //    context.Response.ContentType is not null&&
            //    !context.Response.ContentType.StartsWith("application/")&&
            //    !context.Response.ContentType.StartsWith("image/"))
            if(context.Response.ContentType is not null && context.Response.ContentType.StartsWith("video/"))
            {
                if (context.Request.Headers.TryGetValue("referer", out StringValues s))
                {
                    

                    if (context.Request.Cookies.TryGetValue("accessToken", out string? val))
                    {

                        await Console.Out.WriteLineAsync("aaa");
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
                            //await next(context);
                        }
                        else
                        {
                            context.Response.StatusCode = 401;
                            await context.Response.Body.WriteAsync(new() { });


                        }

                    }
                    else
                    {
                        context.Response.StatusCode = 401;
                        await context.Response.Body.WriteAsync(new() { });


                    }

                   
                }
                else
                {
                    context.Response.StatusCode = 401;
                    await context.Response.Body.WriteAsync(new() { });
                   
                    

                }

            }
            //else if (ext == ".txt")
            //{
            //    if (context.Response.ContentLength is not null)
            //    {
            //        context.Response.ContentType = "text/plain; charset=utf-8";
            //        var path = Path.Combine(webHostEnvironment.WebRootPath, context.Request.Path.Value.TrimStart('/'));
            //        await context.Response.SendFileAsync(path);

            //    }

            //}
           




            //var ext = Path.GetExtension(context.Request.Path.Value);
            //if (ext!=".pdf"&&ext!=".txt"&&ext!=".docx")
            //{
            //    if (context.Request.Headers.TryGetValue("referer", out StringValues s))
            //    {
            //        if (context.Request.Cookies.TryGetValue("accessToken", out string? val))
            //        {


            //            var tokenHandler = new JwtSecurityTokenHandler();

            //            var tvr = await tokenHandler.ValidateTokenAsync(val, new TokenValidationParameters()
            //            {
            //                ValidateAudience = true,
            //                ValidateIssuer = true,
            //                ValidateLifetime = true,
            //                ValidAudience = configuration["JWT:Audience"],
            //                ValidIssuer = configuration["JWT:Issuer"],
            //                ValidateIssuerSigningKey = true,
            //                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:SecretKey"])),
            //                RoleClaimType = ClaimTypes.Role,

            //            }); ;
            //            if (tvr.IsValid)
            //            {
            //                //if(tvr.Claims.Any(x=>x.Key==ClaimTypes.Role&&x.Value=="student"))
            //                await next(context);
            //            }
            //            else
            //            {
            //                context.Response.StatusCode = 401;

            //            }

            //        }
            //        else
            //        {
            //            context.Response.StatusCode = 401;
            //        }

            //        await next(context);
            //    }
            //    else
            //    {
            //        context.Response.StatusCode = 401;
            //    }

            //}
            //else if (ext ==".txt")
            //{
            //    context.Response.ContentType = "text/plain; charset=utf-8";
            //    var path = Path.Combine(webHostEnvironment.WebRootPath, context.Request.Path.Value.TrimStart('/'));
            //    await context.Response.SendFileAsync(path);

            //}
            //else
            //{
            //    await next(context);
            //}




        }
    }
}

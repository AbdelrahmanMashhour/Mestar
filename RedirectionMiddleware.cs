namespace Mestar
{

    //To redirect to front project
    public class RedirectionMiddleware(RequestDelegate next)
    {

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path == "/")

                context.Response.Redirect("/index.html");

            else
            {
             
                    await next(context);
                    if(context.Response.StatusCode==404)
                     context.Response.Redirect("/index.html");


            }




        }
    }
}

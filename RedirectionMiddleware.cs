namespace Mestar
{

    //To redirect to front project
    public class RedirectionMiddleware(RequestDelegate next)
    {

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path == ("/index.html"))
            {
                await next(context);

            }
            else if (context.Request.Path=="/")
            {
                context.Response.Redirect("/index.html");
            }
            else
            {
                await next(context);
            }
            //if (context.Request.Path==("/index.html"))
            //{
            //    await next(context);

            //}
            //else if (context.Request.Path == ("/"))
            //{
            //    context.Response.Redirect("/index.html");
            //}
            //else if (!context.Request.Path.StartsWithSegments("/api"))
            //{
            //    context.Response.Redirect("/");
            //}
            //else 
            //{
            //    await next(context);
            //}
        }
    }
}

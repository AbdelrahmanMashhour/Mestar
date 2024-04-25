
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RepositoryPatternWithUOW.EF;
using RepositoryPatternWithUOW.Core.OptionsPatternClasses;
using RepositoryPatternWithUOW.EfCore.MailService;
using System.Security.Claims;
using System.Text;
using RepositoryPatternWithUOW.EfCore.Mapper;
using RepositoryPatternWithUOW.Core.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Newtonsoft.Json;
using Mestar;
//using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
var connstr = builder.Configuration.GetConnectionString("DefalutConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connstr).UseLazyLoadingProxies());

builder.Services.Configure<TokenOptionsPattern>(builder.Configuration.GetSection("JWT"));

builder.Services.AddControllers().AddNewtonsoftJson(o=>o.SerializerSettings.NullValueHandling=NullValueHandling.Ignore);
    
    //AddJsonOptions(x => x.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull);

builder.Services.AddScoped<Mapper>();
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddScoped<IUnitOfWork,UnitOfWork>();
builder.Services.AddTransient<ISenderService, MailService>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



builder.Services.Configure<KestrelServerOptions>(
    options =>
    {
        options.Limits.MaxRequestBodySize = 104857600;
    });

builder.Services.AddCors(option => {
    option.AddPolicy("Policy",builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var JwtSettings = builder.Configuration.GetSection("JWT").Get<TokenOptionsPattern>();
builder.Services.AddSingleton(JwtSettings!);
builder.Services.AddAuthentication(opts =>
{
    opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(opts =>
    {

        opts.RequireHttpsMetadata = true;
        opts.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateLifetime = true,
            ValidAudience = JwtSettings.Audience,
            ValidIssuer = JwtSettings.Issuer,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSettings.SecretKey)),
            RoleClaimType = ClaimTypes.Role,

        };
        //this part tell user that send token in cookies
        opts.Events = new JwtBearerEvents()
        {
            OnMessageReceived = a =>
        {
            if (a.HttpContext.Request.Cookies.TryGetValue("accessToken",out string? val))
            {
                a.Token = val;

            }
            return Task.CompletedTask;
        }
        };

    });

builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
//options =>
//{
//    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "api.xml"));
//});
var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}


app.UseHttpsRedirection();

app.UseCors("Policy");

app.UseAuthentication();
app.UseAuthorization();

//this middleware to prevent to download videos
app.UseMiddleware<FileMiddleWare>();
app.UseStaticFiles();
app.MapControllers();

app.Run();


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
using Microsoft.AspNetCore.Http.Features;
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


builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 1073741824; // 1 GB in bytes
});





builder.Services.AddCors(option => {
    option.AddPolicy("Policy",builder =>
    {

        builder.WithOrigins("http://localhost:5173", "https://localhost:7155").AllowCredentials().AllowAnyMethod().AllowAnyHeader();

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
        ////this part tell user that send token in cookies

        opts.Events = new JwtBearerEvents()
        {
            OnMessageReceived = a =>
        {
            if (a.HttpContext.Request.Cookies.TryGetValue("accessToken", out string? val))
            {
                a.Token = val;


            }
            return Task.CompletedTask;
        }
        };

    });

builder.Services.AddSwaggerGen(options =>
{

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Educationl_Project",
        Description = " This Project " +
        " Medical Service Monitoring Drivers Health in real time by day ",
        Contact = new OpenApiContact
        {
            Name = "Hossam",
            Email = "HossamHosny415@gmail.com"
        }
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter Your JWT Key "
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});
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
app.UseMiddleware<RedirectionMiddleware>();
//this middleware to prevent to download videos
app.UseMiddleware<FileMiddleWare>();
app.UseStaticFiles();
app.MapControllers();

app.Run();

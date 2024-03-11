
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

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
var connstr = builder.Configuration.GetConnectionString("DefalutConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connstr).UseLazyLoadingProxies());

builder.Services.Configure<TokenOptionsPattern>(builder.Configuration.GetSection("JWT"));
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddScoped<Mapper>();
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddTransient<IUnitOfWork,UnitOfWork>();
builder.Services.AddTransient<ISenderService, MailService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(option => {
    option.AddPolicy("CorsPolicy", builder =>
    {
        builder.AllowAnyHeader().AllowCredentials().AllowAnyMethod().WithOrigins("http://localhost:4200", "https://localhost:4200");
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

    });

builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });

        // Add JWT Bearer token authentication support
        var securityScheme = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "JWT Authorization header using the Bearer scheme",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        };

        c.AddSecurityDefinition("Bearer", securityScheme);

        // Add JWT token authorization header
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] { }
            }
        });
    });
var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}


app.UseAuthentication();
app.UseAuthorization();
app.UseCors(x => x.AllowAnyHeader().AllowAnyOrigin().AllowAnyHeader());
app.MapControllers();

app.Run();

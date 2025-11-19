using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using StudentManagement.Data;
using StudentManagement.Interfaces.Persistence;
using StudentManagement.Interfaces.Services;
using StudentManagement.Middlewares;
using StudentManagement.Models;
using StudentManagement.Repositories;
using StudentManagement.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// Serilog configuration
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// MVC Controllers
builder.Services.AddControllersWithViews();


// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();


// Services
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Email service
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

// JWT Settings
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
var keyBytes = Encoding.UTF8.GetBytes(jwtSettings.Key);


// Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Cookies["jwt"];
            if (!string.IsNullOrEmpty(token))
                context.Token = token;
            return Task.CompletedTask;
        }
    };
});

// Authorization
builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware pipeline

// Global exception handler
app.UseGlobalExceptionHandler();

// Standard middlewares
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// MVC Routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Fallback exception handler for non-development
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.Run();

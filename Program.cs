using System.Text;
using HRM.API.Data;
using HRM.API.Enums;
using HRM.API.Repositories;
using HRM.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// OpenAPI
builder.Services.AddOpenApi();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IDesignationService, DesignationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ILeaveTypeService, LeaveTypeService>();
builder.Services.AddScoped<ILeaveRequestService, LeaveRequestService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IRegularizeAttendanceService, RegularizeAttendanceService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IPerformanceCycleService, PerformanceCycleService>();
builder.Services.AddScoped<IPerformanceRatingService, PerformanceRatingService>();
builder.Services.AddScoped<IPerformanceTemplateService, PerformanceTemplateService>();
builder.Services.AddScoped<IPerformanceCategoryService, PerformanceCategoryService>();
builder.Services.AddScoped<IPerformanceSkillService, PerformanceSkillService>();
builder.Services.AddScoped<IEmployeePerformanceReviewService, EmployeePerformanceReviewService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IReportService, ReportService>();

// Repositories
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IDesignationRepository, DesignationRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<ILeaveTypeRepository, LeaveTypeRepository>();
builder.Services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<IRegularizeAttendanceRepository, RegularizeAttendanceRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IPerformanceCycleRepository, PerformanceCycleRepository>();
builder.Services.AddScoped<IPerformanceRatingRepository, PerformanceRatingRepository>();
builder.Services.AddScoped<IPerformanceTemplateRepository, PerformanceTemplateRepository>();
builder.Services.AddScoped<IPerformanceCategoryRepository, PerformanceCategoryRepository>();
builder.Services.AddScoped<IPerformanceSkillRepository, PerformanceSkillRepository>();
builder.Services.AddScoped<IEmployeePerformanceReviewRepository, EmployeePerformanceReviewRepository>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://localhost:5174")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Development tools
if (app.Environment.IsDevelopment())
{
    // OpenAPI JSON
    app.MapOpenApi();

    // Scalar UI
    app.MapScalarApiReference(options =>
    {
        options.Title = "HRM API";
    });
}

// Middleware
app.UseCors("AllowReactApp");

app.UseHttpsRedirection();

// Serve uploaded files (e.g. profile pictures) from wwwroot
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "uploads", "profile-pictures");
Directory.CreateDirectory(uploadsPath);
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// Controllers
app.MapControllers();

app.Run();
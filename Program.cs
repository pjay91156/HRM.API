using System.Text;
using HRM.API.Data;
using HRM.API.Enums;
using HRM.API.Repositories;
using HRM.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using HRM.API.AI.Services;
using HRM.API.AI.Plugins;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using HRM.API.Hubs;

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
builder.Services.AddScoped<IEmailService, EmailService>();
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
builder.Services.AddScoped<IFloorService, FloorService>();
builder.Services.AddScoped<IMeetingRoomAmenityService, MeetingRoomAmenityService>();
builder.Services.AddScoped<IMeetingRoomService, MeetingRoomService>();
builder.Services.AddScoped<IMeetingRoomBookingService, MeetingRoomBookingService>();
builder.Services.AddScoped<ICompanyDocumentService, CompanyDocumentService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IHolidayService, HolidayService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ICourseModuleService, CourseModuleService>();
builder.Services.AddScoped<IModuleQuizService, ModuleQuizService>();
builder.Services.AddScoped<ILearningService, LearningService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddScoped<LeavePlugin>();
builder.Services.AddScoped<IAIService, AIService>();

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
builder.Services.AddScoped<IFloorRepository, FloorRepository>();
builder.Services.AddScoped<IMeetingRoomAmenityRepository, MeetingRoomAmenityRepository>();
builder.Services.AddScoped<IMeetingRoomRepository, MeetingRoomRepository>();
builder.Services.AddScoped<IMeetingRoomBookingRepository, MeetingRoomBookingRepository>();
builder.Services.AddScoped<ICompanyDocumentRepository, CompanyDocumentRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IHolidayRepository, HolidayRepository>();
builder.Services.AddScoped<IEmployeeOptionalHolidaySelectionRepository, EmployeeOptionalHolidaySelectionRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ICourseModuleRepository, CourseModuleRepository>();
builder.Services.AddScoped<IModuleQuizRepository, ModuleQuizRepository>();
builder.Services.AddScoped<ILearningRepository, LearningRepository>();

builder.Services.AddSignalR();


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

        // SignalR's JS client can't set the Authorization header on
        // WebSocket/SSE upgrade requests, so it sends the access token
        // via the "access_token" query string param instead. Without this,
        // the hub connection is rejected with 401 after negotiate succeeds.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSingleton<Kernel>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();

    var builder = Kernel.CreateBuilder();

    builder.AddOpenAIChatCompletion(
        modelId: configuration["OpenAI:Model"]!,
        apiKey: configuration["OpenAI:ApiKey"]!);

    return builder.Build();
});
builder.Services.AddSingleton<IChatCompletionService>(serviceProvider =>
{
    var kernel = serviceProvider.GetRequiredService<Kernel>();

    return kernel.GetRequiredService<IChatCompletionService>();
});

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
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
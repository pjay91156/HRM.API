using HRM.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Company> Companies { get; set; }

    public DbSet<User> Users { get; set; }
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Designation> Designations => Set<Designation>();
    public DbSet<LeaveType> LeaveTypes { get; set; }
    public DbSet<LeaveRequest> LeaveRequests{get;set;}
    public DbSet<Attendance> Attendances{get;set;}
     public DbSet<AttendanceSession> AttendanceSessions{get;set;}
     public DbSet<AttendanceRegularization> AttendanceRegularizations { get; set; }
     public DbSet<Notification> Notifications => Set<Notification>();
      public DbSet<PerformanceCycle> PerformanceCycles => Set<PerformanceCycle>();
      public DbSet<PerformanceRating> PerformanceRatings => Set<PerformanceRating>();
      public DbSet<PerformanceTemplate> PerformanceTemplates => Set<PerformanceTemplate>();
      public DbSet<PerformanceCategory> PerformanceCategories => Set<PerformanceCategory>();
      public DbSet<PerformanceSkill> PerformanceSkills => Set<PerformanceSkill>();
      public DbSet<EmployeePerformanceReview> EmployeePerformanceReviews => Set<EmployeePerformanceReview>();
      public DbSet<EmployeePerformanceSkillReview> EmployeePerformanceSkillReviews => Set<EmployeePerformanceSkillReview>();
      public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
      public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
      public DbSet<LoginOtp> LoginOtps => Set<LoginOtp>();
      public DbSet<Floor> Floors => Set<Floor>();
      public DbSet<MeetingRoom> MeetingRooms => Set<MeetingRoom>();
      public DbSet<MeetingRoomAmenity> MeetingRoomAmenities => Set<MeetingRoomAmenity>();
      public DbSet<MeetingRoomAmenityDetail> MeetingRoomAmenityDetails => Set<MeetingRoomAmenityDetail>();
      public DbSet<MeetingRoomBooking> MeetingRoomBookings => Set<MeetingRoomBooking>();
      public DbSet<CompanyDocument> CompanyDocuments => Set<CompanyDocument>();
      public DbSet<Holiday> Holidays => Set<Holiday>();
      public DbSet<EmployeeOptionalHolidaySelection> EmployeeOptionalHolidaySelections => Set<EmployeeOptionalHolidaySelection>();
      public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
      public DbSet<Course> Courses => Set<Course>();
      public DbSet<CourseModule> CourseModules => Set<CourseModule>();
      public DbSet<ModuleQuiz> ModuleQuizzes => Set<ModuleQuiz>();
      public DbSet<ModuleQuizQuestion> ModuleQuizQuestions => Set<ModuleQuizQuestion>();
      public DbSet<ModuleQuizOption> ModuleQuizOptions => Set<ModuleQuizOption>();
      public DbSet<ModuleQuizAttempt> ModuleQuizAttempts => Set<ModuleQuizAttempt>();
      public DbSet<ModuleQuizAnswer> ModuleQuizAnswers => Set<ModuleQuizAnswer>();
      public DbSet<ModuleQuizAnswerOption> ModuleQuizAnswerOptions => Set<ModuleQuizAnswerOption>();
      public DbSet<ModuleProgress> ModuleProgress => Set<ModuleProgress>();
      public DbSet<CourseProgress> CourseProgress => Set<CourseProgress>();
}
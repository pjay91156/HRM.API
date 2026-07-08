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
}
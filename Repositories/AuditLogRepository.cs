using HRM.API.Data;
using HRM.API.Enums;
using HRM.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log);
    Task<(int TotalRecords, List<AuditLog> Records)> GetPagedAsync(Guid companyId, AuditLogFilterDto filter);
    Task<AuditLog?> GetByIdAsync(Guid id, Guid companyId);
    Task<(int Total, int Today, int EmployeeChanges, int LeaveActivities)> GetSummaryAsync(Guid companyId);
}

public class AuditLogRepository : IAuditLogRepository
{
    private readonly ApplicationDbContext _context;

    public AuditLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuditLog log)
    {
        await _context.AuditLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }

    public async Task<(int TotalRecords, List<AuditLog> Records)> GetPagedAsync(Guid companyId, AuditLogFilterDto filter)
    {
        var query = _context.AuditLogs
            .AsNoTracking()
            .Where(x => x.CompanyId == companyId);

        if (filter.FromDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= filter.FromDate.Value.Date);
        }

        if (filter.ToDate.HasValue)
        {
            var toDateExclusive = filter.ToDate.Value.Date.AddDays(1);
            query = query.Where(x => x.CreatedAt < toDateExclusive);
        }

        if (filter.PerformedByEmployeeId.HasValue)
        {
            query = query.Where(x => _context.Employees.Any(e =>
                e.Id == filter.PerformedByEmployeeId.Value && e.UserId == x.UserId));
        }

        if (filter.TargetUserId.HasValue)
        {
            query = query.Where(x => x.TargetUserId == filter.TargetUserId.Value);
        }

        if (filter.Module.HasValue)
        {
            query = query.Where(x => x.Module == filter.Module.Value);
        }

        if (filter.Action.HasValue)
        {
            query = query.Where(x => x.Action == filter.Action.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            query = query.Where(x =>
                x.UserName.Contains(search) ||
                (x.TargetUserName != null && x.TargetUserName.Contains(search)) ||
                x.Description.Contains(search));
        }

        var totalRecords = await query.CountAsync();

        var records = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageLength)
            .Take(filter.PageLength)
            .ToListAsync();

        return (totalRecords, records);
    }

    public async Task<AuditLog?> GetByIdAsync(Guid id, Guid companyId)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId);
    }

    public async Task<(int Total, int Today, int EmployeeChanges, int LeaveActivities)> GetSummaryAsync(Guid companyId)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var baseQuery = _context.AuditLogs.AsNoTracking().Where(x => x.CompanyId == companyId);

        var total = await baseQuery.CountAsync();
        var todayCount = await baseQuery.CountAsync(x => x.CreatedAt >= today && x.CreatedAt < tomorrow);
        var employeeChanges = await baseQuery.CountAsync(x => x.Module == AuditModule.Employee);
        var leaveActivities = await baseQuery.CountAsync(x => x.Module == AuditModule.Leave);

        return (total, todayCount, employeeChanges, leaveActivities);
    }
}

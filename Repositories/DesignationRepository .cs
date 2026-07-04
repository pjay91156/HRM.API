using HRM.API.Data;
using HRM.API.Models;
using HRM.API.Responses;
using Microsoft.EntityFrameworkCore;

public interface IDesignationRepository
{
    Task<IEnumerable<DesignationResponse>> GetDesignationsByDepartmentIdAsync(Guid departmentId);
    Task<IEnumerable<DesignationResponse>> GetDesignationsAsync();
    Task<bool> DeactivateAsync(Guid id, Guid userId);
    Task SaveChangesAsync();
    Task<Designation?> GetByIdAsync(Guid id);
}
public class DesignationRepository : IDesignationRepository
{
    private readonly ApplicationDbContext _context;

    public DesignationRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<Designation?> GetByIdAsync(Guid id)
{
    return await _context.Designations
        .FirstOrDefaultAsync(x => x.Id == id);
}

public async Task SaveChangesAsync()
{
    await _context.SaveChangesAsync();
}

    public async Task<IEnumerable<DesignationResponse>> GetDesignationsByDepartmentIdAsync(Guid departmentId)
    {
        return await _context.Designations
            .Where(x => x.DepartmentId == departmentId && x.IsActive)
            .OrderBy(x => x.DesignationName)
            .Select(x => new DesignationResponse
            {
            Id = x.Id,
            DesignationName = x.DesignationName,
            DepartmentId = x.DepartmentId,
            DepartmentName = x.Department.DepartmentName
            })
            .ToListAsync();
    }
    public async Task<IEnumerable<DesignationResponse>> GetDesignationsAsync()
    {
        return await _context.Designations 
        .Where(x =>   x.IsActive)           
            .OrderBy(x => x.DesignationName)
            .Select(x => new DesignationResponse
            {
            Id = x.Id,
            DesignationName = x.DesignationName,
            DepartmentId = x.DepartmentId,
            DepartmentName = x.Department.DepartmentName
            })
            .ToListAsync();
    }
    public async Task<bool> DeactivateAsync(Guid id, Guid userId)
{
    var designation = await _context.Designations
        .FirstOrDefaultAsync(x => x.Id == id);

    if (designation == null)
        return false;

    designation.IsActive = false;
    designation.UpdatedBy = userId;
    designation.UpdatedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync();

    return true;
}
}
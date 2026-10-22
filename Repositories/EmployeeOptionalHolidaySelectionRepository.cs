using HRM.API.Data;
using HRM.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Repositories;

public interface IEmployeeOptionalHolidaySelectionRepository
{
    Task<List<Guid>> GetSelectedHolidayIdsAsync(Guid employeeId, int year);

    Task ReplaceSelectionsAsync(Guid employeeId, int year, List<EmployeeOptionalHolidaySelection> selections);
}

public class EmployeeOptionalHolidaySelectionRepository : IEmployeeOptionalHolidaySelectionRepository
{
    private readonly ApplicationDbContext _context;

    public EmployeeOptionalHolidaySelectionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Guid>> GetSelectedHolidayIdsAsync(Guid employeeId, int year)
    {
        return await _context.EmployeeOptionalHolidaySelections
            .AsNoTracking()
            .Where(x => x.IsActive && x.EmployeeId == employeeId && x.Year == year)
            .Select(x => x.HolidayId)
            .ToListAsync();
    }

    public async Task ReplaceSelectionsAsync(Guid employeeId, int year, List<EmployeeOptionalHolidaySelection> selections)
    {
        var existing = await _context.EmployeeOptionalHolidaySelections
            .Where(x => x.EmployeeId == employeeId && x.Year == year)
            .ToListAsync();

        _context.EmployeeOptionalHolidaySelections.RemoveRange(existing);

        if (selections.Count > 0)
        {
            await _context.EmployeeOptionalHolidaySelections.AddRangeAsync(selections);
        }

        await _context.SaveChangesAsync();
    }
}

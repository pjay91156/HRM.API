using HRM.API.Data;
using HRM.API.Enums;
using HRM.API.Models;
using HRM.API.Responses;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Repositories;

public interface IHolidayRepository
{
    Task<List<HolidayResponse>> GetHolidaysAsync(int? year, HolidayType? holidayType, string? location, string? search);

    Task<Holiday?> GetByIdAsync(Guid id);

    Task AddAsync(Holiday holiday);

    void Update(Holiday holiday);

    Task<bool> DeleteAsync(Guid id);

    Task<List<DateTime>> GetHolidayDatesAsync(DateTime fromDate, DateTime toDate);

    Task<List<HolidayResponse>> GetUpcomingAsync(int count);

    Task<(int total, int publicCount, int optionalCount, int companyCount)> GetDashboardCountsAsync(int year);

    Task<List<Holiday>> GetActiveOptionalHolidaysByYearAsync(int year);

    Task SaveChangesAsync();
}

public class HolidayRepository : IHolidayRepository
{
    private readonly ApplicationDbContext _context;

    public HolidayRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    private static HolidayResponse ToResponse(Holiday holiday)
    {
        return new HolidayResponse
        {
            Id = holiday.Id,
            Name = holiday.Name,
            HolidayDate = holiday.HolidayDate,
            HolidayType = (int)holiday.HolidayType,
            HolidayTypeName = holiday.HolidayType.ToString(),
            Location = holiday.Location,
            Description = holiday.Description,
            ImageUrl = holiday.ImageUrl,
            IsActive = holiday.IsActive,
            CreatedAt = holiday.CreatedAt,
            UpdatedAt = holiday.UpdatedAt
        };
    }

    public async Task<List<HolidayResponse>> GetHolidaysAsync(int? year, HolidayType? holidayType, string? location, string? search)
    {
        var query = _context.Holidays
            .AsNoTracking()
            .Where(x => x.IsActive)
            .AsQueryable();

        if (year.HasValue)
        {
            query = query.Where(x => x.HolidayDate.Year == year.Value);
        }

        if (holidayType.HasValue)
        {
            query = query.Where(x => x.HolidayType == holidayType.Value);
        }

        if (!string.IsNullOrWhiteSpace(location) && !location.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(x => x.Location == location);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.Name.Contains(search));
        }

        return await query
            .OrderBy(x => x.HolidayDate)
            .Select(x => new HolidayResponse
            {
                Id = x.Id,
                Name = x.Name,
                HolidayDate = x.HolidayDate,
                HolidayType = (int)x.HolidayType,
                HolidayTypeName = x.HolidayType.ToString(),
                Location = x.Location,
                Description = x.Description,
                ImageUrl = x.ImageUrl,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<Holiday?> GetByIdAsync(Guid id)
    {
        return await _context.Holidays.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(Holiday holiday)
    {
        await _context.Holidays.AddAsync(holiday);
    }

    public void Update(Holiday holiday)
    {
        _context.Holidays.Update(holiday);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var holiday = await _context.Holidays.FirstOrDefaultAsync(x => x.Id == id);

        if (holiday == null)
        {
            return false;
        }

        holiday.IsActive = false;

        return true;
    }

    public async Task<List<DateTime>> GetHolidayDatesAsync(DateTime fromDate, DateTime toDate)
    {
        return await _context.Holidays
            .AsNoTracking()
            .Where(x => x.IsActive && x.HolidayDate.Date >= fromDate.Date && x.HolidayDate.Date <= toDate.Date)
            .Select(x => x.HolidayDate.Date)
            .ToListAsync();
    }

    public async Task<List<HolidayResponse>> GetUpcomingAsync(int count)
    {
        return await _context.Holidays
            .AsNoTracking()
            .Where(x => x.IsActive && x.HolidayDate.Date >= DateTime.UtcNow.Date)
            .OrderBy(x => x.HolidayDate)
            .Take(count)
            .Select(x => new HolidayResponse
            {
                Id = x.Id,
                Name = x.Name,
                HolidayDate = x.HolidayDate,
                HolidayType = (int)x.HolidayType,
                HolidayTypeName = x.HolidayType.ToString(),
                Location = x.Location,
                Description = x.Description,
                ImageUrl = x.ImageUrl,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<(int total, int publicCount, int optionalCount, int companyCount)> GetDashboardCountsAsync(int year)
    {
        var holidays = await _context.Holidays
            .AsNoTracking()
            .Where(x => x.IsActive && x.HolidayDate.Year == year)
            .Select(x => x.HolidayType)
            .ToListAsync();

        var total = holidays.Count;
        var publicCount = holidays.Count(x => x == HolidayType.PublicHoliday);
        var optionalCount = holidays.Count(x => x == HolidayType.OptionalHoliday);
        var companyCount = holidays.Count(x => x == HolidayType.CompanyHoliday);

        return (total, publicCount, optionalCount, companyCount);
    }

    public async Task<List<Holiday>> GetActiveOptionalHolidaysByYearAsync(int year)
    {
        return await _context.Holidays
            .AsNoTracking()
            .Where(x => x.IsActive && x.HolidayType == HolidayType.OptionalHoliday && x.HolidayDate.Year == year)
            .OrderBy(x => x.HolidayDate)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

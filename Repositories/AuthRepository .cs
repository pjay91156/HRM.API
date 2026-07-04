using HRM.API.Data;
using HRM.API.Models;
using HRM.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Repositories;
public interface IAuthRepository
{
    Task<bool> EmailExistsAsync(string email);

    Task AddCompanyAsync(Company company);

    Task AddUserAsync(User user);

    Task SaveChangesAsync();
    Task<User?> GetUserByEmailAsync(string email);
}
public class AuthRepository : IAuthRepository
{
    private readonly ApplicationDbContext _context;

    public AuthRepository(ApplicationDbContext context)
    {
        _context = context;
        
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users
            .AnyAsync(x => x.Email == email);
    }

    public async Task AddCompanyAsync(Company company)
    {
        await _context.Companies.AddAsync(company);
    }

    public async Task AddUserAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == email && x.IsActive);
    }
}
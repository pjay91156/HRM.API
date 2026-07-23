using HRM.API.Data;
using HRM.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Repositories;

public interface ICompanyDocumentRepository
{
    Task<List<CompanyDocument>> GetAllAsync();

    Task<CompanyDocument?> GetByIdAsync(Guid id);

    Task AddAsync(CompanyDocument document);

    void Update(CompanyDocument document);

    Task SaveChangesAsync();
}

public class CompanyDocumentRepository : ICompanyDocumentRepository
{
    private readonly ApplicationDbContext _context;

    public CompanyDocumentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CompanyDocument>> GetAllAsync()
    {
        return await _context.CompanyDocuments
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<CompanyDocument?> GetByIdAsync(Guid id)
    {
        return await _context.CompanyDocuments
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
    }

    public async Task AddAsync(CompanyDocument document)
    {
        await _context.CompanyDocuments.AddAsync(document);
    }

    public void Update(CompanyDocument document)
    {
        _context.CompanyDocuments.Update(document);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

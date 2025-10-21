using Microsoft.EntityFrameworkCore;
using Transactions.Domain.Entities;
using Transactions.Domain.Repositories;
using Transactions.Infrastructure.Data;

namespace Transactions.Infrastructure.Repositories;

public class ImportRepository : IImportRepository
{
    private readonly TransactionDbContext _context;

    public ImportRepository(TransactionDbContext context)
    {
        _context = context;
    }

    public async Task<Import?> GetByHashAsync(string hash)
    {
        return await _context.Imports
            .FirstOrDefaultAsync(i => i.Sha256Hash == hash);
    }

    public async Task<Import?> GetByIdAsync(Guid id)
    {
        return await _context.Imports
            .Include(i => i.Transactions)
            .Include(i => i.ImportErrors)
            .FirstOrDefaultAsync(i => i.ImportId == id);
    }
}
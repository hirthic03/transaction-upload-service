using Microsoft.EntityFrameworkCore;
using Transactions.Domain.Entities;
using Transactions.Domain.Repositories;
using Transactions.Infrastructure.Data;

namespace Transactions.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly TransactionDbContext _context;

    public TransactionRepository(TransactionDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsAsync(
    string? currency, string? status, DateTime? from, DateTime? toDate)
    {
        var query = _context.Transactions.AsQueryable();

        if (!string.IsNullOrEmpty(currency))
        {
            query = query.Where(t => t.CurrencyCode == currency.ToUpper());
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(t => t.StatusCode == status.ToUpper());
        }

        if (from.HasValue)
        {
            query = query.Where(t => t.TransactionDate >= from.Value);
        }

        if (toDate.HasValue)
{
    // Include the entire end date
    var endOfDay = toDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(t => t.TransactionDate <= endOfDay);
        }

        return await query.OrderBy(t => t.TransactionDate).ToListAsync();
    }

    public async Task AddTransactionsAsync(IEnumerable<Transaction> transactions, Import import)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await _context.Imports.AddAsync(import);
            await _context.Transactions.AddRangeAsync(transactions);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Transaction?> GetByIdAsync(string id)
    {
        return await _context.Transactions
            .Include(t => t.Import)
            .FirstOrDefaultAsync(t => t.Id == id);
    }
}
using Transactions.Domain.Entities;

namespace Transactions.Domain.Repositories;

public interface ITransactionRepository
{
    Task<IEnumerable<Transaction>> GetTransactionsAsync(string currency, string status, DateTime? from, DateTime? toDate);
    Task AddTransactionsAsync(IEnumerable<Transaction> transactions, Import import);
    Task<Transaction?> GetByIdAsync(string id);
}
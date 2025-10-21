using Transactions.Domain.Entities;
using Transactions.Domain.Models;

namespace Transactions.Domain.Services;

public interface ITransactionService
{
    Task<ImportResult> ImportTransactionsAsync(Stream fileStream, string fileName);
    Task<IEnumerable<Transaction>> GetTransactionsAsync(string currency, string status, DateTime? from, DateTime? toDate);
}

public class ImportResult
{
    public bool IsSuccess { get; set; }
    public Guid ImportId { get; set; }
    public int RecordCount { get; set; }
    public string? ErrorMessage { get; set; }
    public IReadOnlyList<ValidationError> ValidationErrors { get; init; } = new List<ValidationError>().AsReadOnly();
}

public class ValidationError
{
    public int Row { get; set; }
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
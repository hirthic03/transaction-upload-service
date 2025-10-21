using Transactions.Domain.Models;

namespace Transactions.Domain.Parsers;

public interface IFileParser
{
    string Format { get; }
    Task<ParseResult> ParseAsync(Stream stream);
}

public class ParseResult
{
    public bool IsSuccess { get; set; }
    public IReadOnlyList<TransactionRecord> Records { get; init; } = new List<TransactionRecord>().AsReadOnly();
    public string? ErrorMessage { get; set; }
}
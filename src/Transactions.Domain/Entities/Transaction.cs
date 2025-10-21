namespace Transactions.Domain.Entities;

public class Transaction
{
    public string Id { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string StatusCode { get; set; } = string.Empty;
    public string SourceFormat { get; set; } = string.Empty;
    public Guid ImportId { get; set; }
    public Import Import { get; set; } = null!;
}
namespace Transactions.Domain.Models;

public class TransactionRecord
{
    public string Id { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
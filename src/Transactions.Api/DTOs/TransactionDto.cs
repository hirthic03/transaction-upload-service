namespace Transactions.Api.DTOs;

public class TransactionDto
{
    public string Id { get; set; } = string.Empty;
    public string Payment { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
namespace Transactions.Api.DTOs;

public class ValidationErrorDto
{
    public int Row { get; set; }
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
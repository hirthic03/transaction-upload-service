namespace Transactions.Domain.Entities;

public class ImportError
{
    public int Id { get; set; }
    public Guid ImportId { get; set; }
    public int RowNumber { get; set; }
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Import Import { get; set; } = null!;
}
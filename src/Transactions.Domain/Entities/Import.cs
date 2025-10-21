namespace Transactions.Domain.Entities;

public class Import
{
    public Guid ImportId { get; set; }
    public DateTime ReceivedAt { get; set; }
    public string SourceFormat { get; set; } = string.Empty;
    public string Sha256Hash { get; set; } = string.Empty;
    public int RecordCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public ICollection<Transaction> Transactions { get; } = new List<Transaction>();
    public ICollection<ImportError> ImportErrors { get; } = new List<ImportError>();
}
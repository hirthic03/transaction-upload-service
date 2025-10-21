namespace Transactions.Domain.Models;

public class ValidationResult
{
    public bool IsValid { get; }
    public IReadOnlyList<ValidationError> Errors { get; }

    public ValidationResult(IReadOnlyList<ValidationError>? errors = null)
    {
        Errors = errors ?? new List<ValidationError>().AsReadOnly();
        IsValid = Errors.Count == 0;
    }
}

public class ValidationError
{
    public int Row { get; set; }
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
namespace Transactions.Api.DTOs;

public class UploadResponseDto
{
    public string ImportId { get; set; } = string.Empty;
    public int Imported { get; set; }
}
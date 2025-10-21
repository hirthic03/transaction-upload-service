using System.Text.Json.Serialization;

namespace Transactions.Api.DTOs;

public class ErrorResponseDto
{
    public string Error { get; set; } = string.Empty;
}

public class ValidationErrorResponseDto
{
    public List<ValidationErrorDto> Errors { get; set; } = new();
}
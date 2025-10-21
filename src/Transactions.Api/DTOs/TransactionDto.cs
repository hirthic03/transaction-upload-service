using System.Text.Json.Serialization;

namespace Transactions.Api.DTOs;

public class TransactionDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("payment")]
    public string Payment { get; set; } = string.Empty;
    
    [JsonPropertyName("Status")]
    public string Status { get; set; } = string.Empty;
}
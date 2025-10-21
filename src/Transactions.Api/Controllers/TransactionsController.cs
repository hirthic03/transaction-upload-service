using Microsoft.AspNetCore.Mvc;
using Transactions.Api.DTOs;
using Transactions.Domain.Services;

namespace Transactions.Api.Controllers;

[ApiController]
[Route("api/v1")]
[Produces("application/json")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(ITransactionService transactionService, ILogger<TransactionsController> logger)
    {
        _transactionService = transactionService;
        _logger = logger;
    }

    [HttpPost("uploads")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UploadResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status413PayloadTooLarge)]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new ErrorResponseDto { Error = "No file uploaded" });
        }

        const int maxFileSize = 1048576; // 1 MB
        if (file.Length > maxFileSize)
        {
            return StatusCode(413, new ErrorResponseDto { Error = "File too large (max 1 MB)" });
        }

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _transactionService.ImportTransactionsAsync(stream, file.FileName);

            if (!result.IsSuccess)
            {
                if (result.ErrorMessage == "Unknown format")
                {
                    return BadRequest(new ErrorResponseDto { Error = "Unknown format" });
                }

                if (result.ValidationErrors != null && result.ValidationErrors.Any())
                {
                    var errorResponse = new ValidationErrorResponseDto
                    {
                        Errors = result.ValidationErrors?.Select(e => new ValidationErrorDto
                        {
                            Row = e.Row,
                            Field = e.Field,
                            Message = e.Message
                        }).ToList() ?? new List<ValidationErrorDto>()
                    };
                    return BadRequest(errorResponse);
                }

                return BadRequest(new ErrorResponseDto { Error = result.ErrorMessage ?? "An error occurred" });
            }

            var response = new UploadResponseDto
            {
                ImportId = result.ImportId.ToString(),
                Imported = result.RecordCount
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during file upload");
            throw;
        }
    }

    [HttpGet("transactions")]
    [ProducesResponseType(typeof(List<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] string? currency = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var transactions = await _transactionService.GetTransactionsAsync(
            currency ?? string.Empty, 
            status ?? string.Empty, 
            from, 
            to);
        
        var response = transactions.Select(t => new TransactionDto
        {
            Id = t.Id,
            Payment = $"{t.Amount:F2} {t.CurrencyCode}",
            Status = t.StatusCode
        }).ToList();

        return Ok(response);
    }
}
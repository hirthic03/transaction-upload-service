using System.Security.Cryptography;
using System.Text;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Transactions.Domain.Entities;
using Transactions.Domain.Models;
using Transactions.Domain.Parsers;
using Transactions.Domain.Repositories;
using System.Globalization;

namespace Transactions.Domain.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IImportRepository _importRepository;
    private readonly IEnumerable<IFileParser> _parsers;
    private readonly IValidator<TransactionRecord> _validator;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(
        ITransactionRepository transactionRepository,
        IImportRepository importRepository,
        IEnumerable<IFileParser> parsers,
        IValidator<TransactionRecord> validator,
        ILogger<TransactionService> logger)
    {
        _transactionRepository = transactionRepository;
        _importRepository = importRepository;
        _parsers = parsers;
        _validator = validator;
        _logger = logger;
    }

    public async Task<ImportResult> ImportTransactionsAsync(Stream fileStream, string fileName)
    {
        #pragma warning disable CA1308
        ArgumentNullException.ThrowIfNull(fileStream);
        // Calculate file hash for idempotency
        var hash = await CalculateHashAsync(fileStream).ConfigureAwait(false);
        fileStream.Position = 0;

        // Check if file already imported
        var existingImport = await _importRepository.GetByHashAsync(hash).ConfigureAwait(false);
        if (existingImport != null)
        {
            _logger.LogInformation("File already imported with ImportId: {ImportId}", existingImport.ImportId);
            return new ImportResult
            {
                IsSuccess = true,
                ImportId = existingImport.ImportId,
                RecordCount = existingImport.RecordCount
            };
        }

        // Determine file format and select parser
        var parser = SelectParser(fileName);
        if (parser == null)
        {
            return new ImportResult
            {
                IsSuccess = false,
                ErrorMessage = "Unknown format"
            };
        }

        // Parse file
        var parseResult = await parser.ParseAsync(fileStream).ConfigureAwait(false);
        if (!parseResult.IsSuccess)
        {
            return new ImportResult
            {
                IsSuccess = false,
                ErrorMessage = parseResult.ErrorMessage
            };
        }

        // Validate all records
        var validationErrors = new List<ValidationError>();
        var rowNumber = 1;
        foreach (var record in parseResult.Records)
        {
            var validationResult = await _validator.ValidateAsync(record).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    validationErrors.Add(new ValidationError
                    {
                        Row = rowNumber,
                        Field = error.PropertyName.ToLowerInvariant(),
                        Message = error.ErrorMessage
                    });
                }
            }
            rowNumber++;
        }

        if (validationErrors.Count > 0)
        {
            return new ImportResult
            {
                IsSuccess = false,
                ValidationErrors = validationErrors.AsReadOnly()
            };
        }

        // Create import record
        var import = new Import
        {
            ImportId = Guid.NewGuid(),
            ReceivedAt = DateTime.UtcNow,
            SourceFormat = parser.Format,
            Sha256Hash = hash,
            RecordCount = parseResult.Records.Count,
            Status = "Success"
        };

        // Map records to transactions
        var transactions = parseResult.Records.Select(r => new Transaction
        {
            Id = r.Id,
            Amount = r.Amount,
            CurrencyCode = r.CurrencyCode,
            TransactionDate = r.TransactionDate,
            StatusCode = MapStatusCode(r.Status),
            SourceFormat = parser.Format,
            ImportId = import.ImportId
        }).ToList();

        // Save in single transaction
        await _transactionRepository.AddTransactionsAsync(transactions, import).ConfigureAwait(false);

        return new ImportResult
        {
            IsSuccess = true,
            ImportId = import.ImportId,
            RecordCount = transactions.Count
        };
        #pragma warning restore CA1308
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsAsync(string currency, string status, DateTime? from, DateTime? toDate)
{
    return await _transactionRepository.GetTransactionsAsync(currency, status, from, toDate).ConfigureAwait(false);
}

    private IFileParser? SelectParser(string fileName)
{
    var extension = Path.GetExtension(fileName).ToUpperInvariant().Replace(".", "", StringComparison.Ordinal);
    return extension switch
    {
        "CSV" => _parsers.FirstOrDefault(p => p.Format == "CSV"),
        "XML" => _parsers.FirstOrDefault(p => p.Format == "XML"),
        _ => null
    };
}

    private static async Task<string> CalculateHashAsync(Stream stream)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = await sha256.ComputeHashAsync(stream).ConfigureAwait(false);
        return Convert.ToBase64String(hashBytes);
    }

    private static string MapStatusCode(string status)
{
    return status.ToUpperInvariant() switch
    {
        "APPROVED" => "A",
        "FAILED" => "R",
        "REJECTED" => "R",
        "FINISHED" => "D",
        "DONE" => "D",
        _ => throw new ArgumentException($"Invalid status: {status}")
    };
}
}
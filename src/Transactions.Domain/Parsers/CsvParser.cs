using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using Transactions.Domain.Models;

namespace Transactions.Domain.Parsers;

public class CsvParser : IFileParser
{
    private readonly ILogger<CsvParser> _logger;
    public string Format => "CSV";

    public CsvParser(ILogger<CsvParser> logger)
    {
        _logger = logger;
    }

    public async Task<ParseResult> ParseAsync(Stream stream)
    {
        try
        {
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                BadDataFound = null
            });

            var records = new List<TransactionRecord>();
            await foreach (var row in csv.GetRecordsAsync<dynamic>())
            {
                try
                {
                    var dict = (IDictionary<string, object>)row;
                    if (dict.Count != 5)
                    {
                        return new ParseResult
                        {
                            IsSuccess = false,
                            ErrorMessage = "Invalid CSV format - expected 5 columns"
                        };
                    }

                    var record = new TransactionRecord
                    {
                        Id = dict["TransactionId"]?.ToString()?.Trim('"') ?? string.Empty,
                        Amount = ParseAmount(dict["Amount"]?.ToString() ?? string.Empty),
                        CurrencyCode = dict["CurrencyCode"]?.ToString()?.Trim('"') ?? string.Empty,
                        TransactionDate = ParseDateTime(dict["Date"]?.ToString()?.Trim('"') ?? string.Empty),
                        Status = dict["Status"]?.ToString()?.Trim('"') ?? string.Empty
                    };

                    records.Add(record);
                }
                catch (FormatException ex)
                {
                    _logger.LogError(ex, "Error parsing CSV row");
                    return new ParseResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Error parsing CSV file"
                    };
                }
            }

            return new ParseResult
            {
                IsSuccess = true,
                Records = records
            };
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Error reading CSV file");
            return new ParseResult
            {
                IsSuccess = false,
                ErrorMessage = "Error reading CSV file"
            };
        }
    }

    private static decimal ParseAmount(string amountStr)
    {
        amountStr = amountStr.Trim('"').Replace(",", "", StringComparison.Ordinal);
        if (decimal.TryParse(amountStr, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
        {
            return amount;
        }
        return 0;
    }

    private static DateTime ParseDateTime(string dateStr)
    {
        if (DateTime.TryParseExact(dateStr, "dd/MM/yyyy HH:mm:ss", 
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return date;
        }
        return DateTime.MinValue;
    }
}
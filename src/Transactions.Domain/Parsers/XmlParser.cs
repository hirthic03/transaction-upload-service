using System.Globalization;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Transactions.Domain.Models;
using System.Xml;

namespace Transactions.Domain.Parsers;

public class XmlParser : IFileParser
{
    private readonly ILogger<XmlParser> _logger;
    public string Format => "XML";

    public XmlParser(ILogger<XmlParser> logger)
    {
        _logger = logger;
    }

    public async Task<ParseResult> ParseAsync(Stream stream)
    {
        try
        {
            string content;
            using (var reader = new StreamReader(stream))
            {
                content = await reader.ReadToEndAsync().ConfigureAwait(false);
            }
            var doc = XDocument.Parse(content);
            
            var records = new List<TransactionRecord>();
            var transactions = doc.Descendants("Transaction");

            foreach (var transaction in transactions)
            {
                try
                {
                    var record = new TransactionRecord
                    {
                        Id = transaction.Attribute("id")?.Value ?? string.Empty,
                        TransactionDate = ParseDateTime(transaction.Element("TransactionDate")?.Value ?? string.Empty),
                        Amount = ParseAmount(transaction.Element("PaymentDetails")?.Element("Amount")?.Value ?? string.Empty),
                        CurrencyCode = transaction.Element("PaymentDetails")?.Element("CurrencyCode")?.Value ?? string.Empty,
                        Status = transaction.Element("Status")?.Value ?? string.Empty
                    };

                    records.Add(record);
                }
                catch (XmlException ex)
                    {
                        var transactionId = transaction.Attribute("id")?.Value ?? "unknown";
                        _logger.LogError(ex, "Error parsing XML transaction with id {TransactionId}", transactionId);
                        return new ParseResult
                        {
                            IsSuccess = false,
                            ErrorMessage = "Error parsing XML transaction"
                        };
                    }
            }

            return new ParseResult
            {
                IsSuccess = true,
                Records = records
            };
        }
        catch (XmlException ex)
        {
            _logger.LogError(ex, "Error parsing XML file");
            return new ParseResult
            {
                IsSuccess = false,
                ErrorMessage = "Error parsing XML file"
            };
        }
    }

    private static decimal ParseAmount(string amountStr)
    {
        if (decimal.TryParse(amountStr, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
        {
            return amount;
        }
        return 0;
    }

    private static DateTime ParseDateTime(string dateStr)
    {
        if (DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return date;
        }
        return DateTime.MinValue;
    }
}
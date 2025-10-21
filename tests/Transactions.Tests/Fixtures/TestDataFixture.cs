using Transactions.Domain.Entities;
using Transactions.Domain.Models;

namespace Transactions.Tests.Fixtures;

public static class TestDataFixture
{
    public static List<TransactionRecord> GetValidTransactionRecords()
    {
        return new List<TransactionRecord>
        {
            new() { Id = "INV001", Amount = 100m, CurrencyCode = "USD", Status = "Approved", TransactionDate = DateTime.Parse("2019-01-01") },
            new() { Id = "INV002", Amount = 200m, CurrencyCode = "EUR", Status = "Failed", TransactionDate = DateTime.Parse("2019-01-02") },
            new() { Id = "INV003", Amount = 300m, CurrencyCode = "GBP", Status = "Finished", TransactionDate = DateTime.Parse("2019-01-03") }
        };
    }

    public static string GetValidCsv()
    {
        return "\"INV001\",\"100.00\",\"USD\",\"01/01/2019 12:00:00\",\"Approved\"\n" +
               "\"INV002\",\"200.00\",\"EUR\",\"02/01/2019 12:00:00\",\"Failed\"\n" +
               "\"INV003\",\"300.00\",\"GBP\",\"03/01/2019 12:00:00\",\"Finished\"";
    }

    public static string GetValidXml()
    {
        return @"<Transactions>
            <Transaction id=""INV001"">
                <TransactionDate>2019-01-01T12:00:00</TransactionDate>
                <PaymentDetails>
                    <Amount>100.00</Amount>
                    <CurrencyCode>USD</CurrencyCode>
                </PaymentDetails>
                <Status>Approved</Status>
            </Transaction>
            <Transaction id=""INV002"">
                <TransactionDate>2019-01-02T12:00:00</TransactionDate>
                <PaymentDetails>
                    <Amount>200.00</Amount>
                    <CurrencyCode>EUR</CurrencyCode>
                </PaymentDetails>
                <Status>Rejected</Status>
            </Transaction>
        </Transactions>";
    }
}
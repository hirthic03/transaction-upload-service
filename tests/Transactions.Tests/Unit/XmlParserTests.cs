using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Transactions.Domain.Parsers;
using Xunit;

namespace Transactions.Tests.Unit.Parsers;

public class XmlParserTests
{
    private readonly XmlParser _parser;
    private readonly Mock<ILogger<XmlParser>> _loggerMock;

    public XmlParserTests()
    {
        _loggerMock = new Mock<ILogger<XmlParser>>();
        _parser = new XmlParser(_loggerMock.Object);
    }

    [Fact]
    public async Task ParseAsync_ValidXml_ReturnsSuccess()
    {
        // Arrange
        var xml = @"<Transactions>
            <Transaction id=""Inv00001"">
                <TransactionDate>2019-01-23T13:45:10</TransactionDate>
                <PaymentDetails>
                    <Amount>200.00</Amount>
                    <CurrencyCode>USD</CurrencyCode>
                </PaymentDetails>
                <Status>Done</Status>
            </Transaction>
            <Transaction id=""Inv00002"">
                <TransactionDate>2019-01-24T16:09:15</TransactionDate>
                <PaymentDetails>
                    <Amount>10000.00</Amount>
                    <CurrencyCode>EUR</CurrencyCode>
                </PaymentDetails>
                <Status>Rejected</Status>
            </Transaction>
        </Transactions>";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));

        // Act
        var result = await _parser.ParseAsync(stream);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Records.Should().HaveCount(2);
        result.Records[0].Id.Should().Be("Inv00001");
        result.Records[0].Amount.Should().Be(200.00m);
        result.Records[0].CurrencyCode.Should().Be("USD");
        result.Records[0].Status.Should().Be("Done");
        result.Records[1].Id.Should().Be("Inv00002");
        result.Records[1].Amount.Should().Be(10000.00m);
        result.Records[1].CurrencyCode.Should().Be("EUR");
        result.Records[1].Status.Should().Be("Rejected");
    }

    [Fact]
    public async Task ParseAsync_EmptyTransactions_ReturnsEmptyRecords()
    {
        // Arrange
        var xml = "<Transactions></Transactions>";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));

        // Act
        var result = await _parser.ParseAsync(stream);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Records.Should().BeEmpty();
    }

    [Fact]
    public async Task ParseAsync_InvalidXml_ReturnsError()
    {
        // Arrange
        var xml = "<Transactions><Transaction>Invalid XML";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));

        // Act
        var result = await _parser.ParseAsync(stream);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Error parsing XML");
    }

    [Fact]
    public async Task ParseAsync_MissingElements_HandlesGracefully()
    {
        // Arrange
        var xml = @"<Transactions>
            <Transaction id=""Inv00001"">
                <TransactionDate>2019-01-23T13:45:10</TransactionDate>
                <PaymentDetails>
                    <Amount>200.00</Amount>
                </PaymentDetails>
                <Status>Done</Status>
            </Transaction>
        </Transactions>";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));

        // Act
        var result = await _parser.ParseAsync(stream);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Records[0].CurrencyCode.Should().Be(string.Empty);
    }
}
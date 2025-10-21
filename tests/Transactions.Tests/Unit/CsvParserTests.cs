using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Transactions.Domain.Parsers;
using Xunit;

namespace Transactions.Tests.Unit.Parsers;

public class CsvParserTests
{
    private readonly CsvParser _parser;
    private readonly Mock<ILogger<CsvParser>> _loggerMock;

    public CsvParserTests()
    {
        _loggerMock = new Mock<ILogger<CsvParser>>();
        _parser = new CsvParser(_loggerMock.Object);
    }

    [Fact]
    public async Task ParseAsync_ValidCsv_ReturnsSuccess()
    {
        // Arrange
        var csv = "\"Invoice0000001\",\"1,000.00\", \"USD\", \"20/02/2019 12:33:16\", \"Approved\"\n" +
                  "\"Invoice0000002\",\"300.00\",\"USD\",\"21/02/2019 02:04:59\", \"Failed\"";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        // Act
        var result = await _parser.ParseAsync(stream);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Records.Should().HaveCount(2);
        result.Records[0].Id.Should().Be("Invoice0000001");
        result.Records[0].Amount.Should().Be(1000.00m);
        result.Records[0].CurrencyCode.Should().Be("USD");
        result.Records[0].Status.Should().Be("Approved");
        result.Records[1].Id.Should().Be("Invoice0000002");
        result.Records[1].Amount.Should().Be(300.00m);
    }

    [Fact]
    public async Task ParseAsync_EmptyFile_ReturnsEmptyRecords()
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(string.Empty));

        // Act
        var result = await _parser.ParseAsync(stream);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Records.Should().BeEmpty();
    }

    [Fact]
    public async Task ParseAsync_InvalidColumns_ReturnsError()
    {
        // Arrange
        var csv = "\"Invoice0000001\",\"1,000.00\", \"USD\""; // Missing columns
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        // Act
        var result = await _parser.ParseAsync(stream);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid CSV format");
    }

    [Fact]
    public async Task ParseAsync_HandlesCommaInAmount_ParsesCorrectly()
    {
        // Arrange
        var csv = "\"Invoice0000001\",\"10,000.00\", \"USD\", \"20/02/2019 12:33:16\", \"Approved\"";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        // Act
        var result = await _parser.ParseAsync(stream);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Records[0].Amount.Should().Be(10000.00m);
    }

    [Fact]
    public async Task ParseAsync_TrimsWhitespace_ReturnsCleanData()
    {
        // Arrange
        var csv = "\"  Invoice0000001  \",\" 1,000.00 \", \"  USD  \", \"20/02/2019 12:33:16\", \"  Approved  \"";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        // Act
        var result = await _parser.ParseAsync(stream);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Records[0].Id.Should().Be("Invoice0000001");
        result.Records[0].CurrencyCode.Should().Be("USD");
        result.Records[0].Status.Should().Be("Approved");
    }
}
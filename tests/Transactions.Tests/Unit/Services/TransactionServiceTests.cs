using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Transactions.Domain.Entities;
using Transactions.Domain.Models;
using Transactions.Domain.Parsers;
using Transactions.Domain.Repositories;
using Transactions.Domain.Services;
using Xunit;

namespace Transactions.Tests.Unit.Services;

public class TransactionServiceTests
{
    private readonly Mock<ITransactionRepository> _transactionRepoMock;
    private readonly Mock<IImportRepository> _importRepoMock;
    private readonly Mock<IFileParser> _csvParserMock;
    private readonly Mock<IFileParser> _xmlParserMock;
    private readonly Mock<IValidator<TransactionRecord>> _validatorMock;
    private readonly Mock<ILogger<TransactionService>> _loggerMock;
    private readonly TransactionService _service;

    public TransactionServiceTests()
    {
        _transactionRepoMock = new Mock<ITransactionRepository>();
        _importRepoMock = new Mock<IImportRepository>();
        _csvParserMock = new Mock<IFileParser>();
        _xmlParserMock = new Mock<IFileParser>();
        _validatorMock = new Mock<IValidator<TransactionRecord>>();
        _loggerMock = new Mock<ILogger<TransactionService>>();

        _csvParserMock.Setup(p => p.Format).Returns("CSV");
        _xmlParserMock.Setup(p => p.Format).Returns("XML");

        var parsers = new List<IFileParser> { _csvParserMock.Object, _xmlParserMock.Object };

        _service = new TransactionService(
            _transactionRepoMock.Object,
            _importRepoMock.Object,
            parsers,
            _validatorMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ImportTransactionsAsync_UnknownFormat_ReturnsError()
    {
        // Arrange
        var stream = new MemoryStream();

        // Act
        var result = await _service.ImportTransactionsAsync(stream, "file.txt");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Unknown format");
    }

    [Fact]
    public async Task ImportTransactionsAsync_DuplicateFile_ReturnsExistingImport()
    {
        // Arrange
        var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var existingImport = new Import
        {
            ImportId = Guid.NewGuid(),
            RecordCount = 5
        };

        _importRepoMock.Setup(r => r.GetByHashAsync(It.IsAny<string>()))
            .ReturnsAsync(existingImport);

        // Act
        var result = await _service.ImportTransactionsAsync(stream, "file.csv");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.ImportId.Should().Be(existingImport.ImportId);
        result.RecordCount.Should().Be(5);
    }

    [Fact]
    public async Task ImportTransactionsAsync_ValidCsv_ImportsSuccessfully()
    {
        // Arrange
        var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var records = new List<TransactionRecord>
        {
            new() { Id = "INV001", Amount = 100m, CurrencyCode = "USD", Status = "Approved", TransactionDate = DateTime.UtcNow }
        };

        _importRepoMock.Setup(r => r.GetByHashAsync(It.IsAny<string>()))
            .ReturnsAsync((Import?)null);

        _csvParserMock.Setup(p => p.ParseAsync(It.IsAny<Stream>()))
            .ReturnsAsync(new ParseResult { IsSuccess = true, Records = records });

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<TransactionRecord>(), default))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _service.ImportTransactionsAsync(stream, "file.csv");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.RecordCount.Should().Be(1);
        _transactionRepoMock.Verify(r => r.AddTransactionsAsync(
            It.IsAny<IEnumerable<Transaction>>(), 
            It.IsAny<Import>()), Times.Once);
    }

    [Fact]
    public async Task ImportTransactionsAsync_ValidationError_ReturnsErrors()
    {
        // Arrange
        var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var records = new List<TransactionRecord>
        {
            new() { Id = "", Amount = 100m, CurrencyCode = "USD", Status = "Approved", TransactionDate = DateTime.UtcNow }
        };

        _importRepoMock.Setup(r => r.GetByHashAsync(It.IsAny<string>()))
            .ReturnsAsync((Import?)null);

        _csvParserMock.Setup(p => p.ParseAsync(It.IsAny<Stream>()))
            .ReturnsAsync(new ParseResult { IsSuccess = true, Records = records });

        var validationResult = new ValidationResult(new[]
        {
            new ValidationFailure("Id", "Transaction ID is required")
        });

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<TransactionRecord>(), default))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _service.ImportTransactionsAsync(stream, "file.csv");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().HaveCount(1);
        result.ValidationErrors[0].Field.Should().Be("id");
        result.ValidationErrors[0].Message.Should().Be("Transaction ID is required");
    }
}
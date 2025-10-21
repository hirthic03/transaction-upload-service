using FluentAssertions;
using Transactions.Domain.Models;
using Transactions.Domain.Validators;
using Xunit;

namespace Transactions.Tests.Unit.Validators;

public class TransactionValidatorTests
{
    private readonly TransactionValidator _validator;

    public TransactionValidatorTests()
    {
        _validator = new TransactionValidator();
    }

    [Fact]
    public async Task Validate_ValidRecord_ReturnsValid()
    {
        // Arrange
        var record = new TransactionRecord
        {
            Id = "INV001",
            Amount = 100.00m,
            CurrencyCode = "USD",
            TransactionDate = DateTime.UtcNow.AddDays(-1),
            Status = "Approved"
        };

        // Act
        var result = await _validator.ValidateAsync(record);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_EmptyId_ReturnsInvalid()
    {
        // Arrange
        var record = new TransactionRecord
        {
            Id = "",
            Amount = 100.00m,
            CurrencyCode = "USD",
            TransactionDate = DateTime.UtcNow,
            Status = "Approved"
        };

        // Act
        var result = await _validator.ValidateAsync(record);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id");
    }

    [Fact]
    public async Task Validate_IdTooLong_ReturnsInvalid()
    {
        // Arrange
        var record = new TransactionRecord
        {
            Id = new string('A', 51),
            Amount = 100.00m,
            CurrencyCode = "USD",
            TransactionDate = DateTime.UtcNow,
            Status = "Approved"
        };

        // Act
        var result = await _validator.ValidateAsync(record);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id" && e.ErrorMessage.Contains("50 characters"));
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("GBP")]
    public async Task Validate_ValidCurrencyCode_ReturnsValid(string currencyCode)
    {
        // Arrange
        var record = new TransactionRecord
        {
            Id = "INV001",
            Amount = 100.00m,
            CurrencyCode = currencyCode,
            TransactionDate = DateTime.UtcNow,
            Status = "Approved"
        };

        // Act
        var result = await _validator.ValidateAsync(record);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("XXX")]
    [InlineData("US")]
    [InlineData("USDD")]
    [InlineData("")]
    public async Task Validate_InvalidCurrencyCode_ReturnsInvalid(string currencyCode)
    {
        // Arrange
        var record = new TransactionRecord
        {
            Id = "INV001",
            Amount = 100.00m,
            CurrencyCode = currencyCode,
            TransactionDate = DateTime.UtcNow,
            Status = "Approved"
        };

        // Act
        var result = await _validator.ValidateAsync(record);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("Approved")]
    [InlineData("Failed")]
    [InlineData("Finished")]
    [InlineData("Rejected")]
    [InlineData("Done")]
    public async Task Validate_ValidStatus_ReturnsValid(string status)
    {
        // Arrange
        var record = new TransactionRecord
        {
            Id = "INV001",
            Amount = 100.00m,
            CurrencyCode = "USD",
            TransactionDate = DateTime.UtcNow,
            Status = status
        };

        // Act
        var result = await _validator.ValidateAsync(record);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_FutureDate_ReturnsInvalid()
    {
        // Arrange
        var record = new TransactionRecord
        {
            Id = "INV001",
            Amount = 100.00m,
            CurrencyCode = "USD",
            TransactionDate = DateTime.UtcNow.AddDays(2),
            Status = "Approved"
        };

        // Act
        var result = await _validator.ValidateAsync(record);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TransactionDate");
    }
}
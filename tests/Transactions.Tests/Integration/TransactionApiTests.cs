using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Transactions.Api.DTOs;
using Xunit;

namespace Transactions.Tests.Integration;

public class TransactionApiTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public TransactionApiTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task PostUpload_ValidCsvFile_Returns200()
    {
        // Arrange
        var csv = "\"Invoice0000001\",\"1,000.00\", \"USD\", \"20/02/2019 12:33:16\", \"Approved\"";
        var content = new MultipartFormDataContent();
        content.Add(new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(csv))), "file", "test.csv");

        // Act
        var response = await _client.PostAsync("/api/v1/uploads", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<UploadResponseDto>();
        result.Should().NotBeNull();
        result!.Imported.Should().Be(1);
    }

    [Fact]
    public async Task PostUpload_UnknownFormat_Returns400()
    {
        // Arrange
        var content = new MultipartFormDataContent();
        content.Add(new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("test"))), "file", "test.txt");

        // Act
        var response = await _client.PostAsync("/api/v1/uploads", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var result = await response.Content.ReadFromJsonAsync<ErrorResponseDto>();
        result!.Error.Should().Be("Unknown format");
    }

    [Fact]
    public async Task PostUpload_InvalidCsv_Returns400WithErrors()
    {
        // Arrange
        var csv = "\"InvalidId123456789012345678901234567890123456789012345\",\"invalid\", \"XXX\", \"invalid\", \"Invalid\"";
        var content = new MultipartFormDataContent();
        content.Add(new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(csv))), "file", "test.csv");

        // Act
        var response = await _client.PostAsync("/api/v1/uploads", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var result = await response.Content.ReadFromJsonAsync<ValidationErrorResponseDto>();
        result!.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task PostUpload_FileTooLarge_Returns413()
    {
        // Arrange
        var largeFile = new byte[1048577]; // 1 MB + 1 byte
        var content = new MultipartFormDataContent();
        content.Add(new StreamContent(new MemoryStream(largeFile)), "file", "test.csv");

        // Act
        var response = await _client.PostAsync("/api/v1/uploads", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.PayloadTooLarge);
    }

    [Fact]
    public async Task GetTransactions_NoFilters_ReturnsAllTransactions()
    {
        // Arrange - Upload a file first
        var csv = "\"Invoice0000001\",\"1,000.00\", \"USD\", \"20/02/2019 12:33:16\", \"Approved\"\n" +
                  "\"Invoice0000002\",\"300.00\",\"EUR\",\"21/02/2019 02:04:59\", \"Failed\"";
        var content = new MultipartFormDataContent();
        content.Add(new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(csv))), "file", "test.csv");
        await _client.PostAsync("/api/v1/uploads", content);

        // Act
        var response = await _client.GetAsync("/api/v1/transactions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<TransactionDto>>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTransactions_WithCurrencyFilter_ReturnsFilteredTransactions()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/transactions?currency=USD");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<TransactionDto>>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTransactions_WithStatusFilter_ReturnsFilteredTransactions()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/transactions?status=A");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<TransactionDto>>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTransactions_WithDateRange_ReturnsFilteredTransactions()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/transactions?from=2019-01-01&to=2019-12-31");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<TransactionDto>>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetHealth_ReturnsHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetMetrics_ReturnsMetrics()
    {
        // Act
        var response = await _client.GetAsync("/metrics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
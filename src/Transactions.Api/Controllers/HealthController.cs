using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Transactions.Infrastructure.Data;

namespace Transactions.Api.Controllers;

[ApiController]
[Route("")]
public class HealthController : ControllerBase
{
    private readonly TransactionDbContext _dbContext;

    public HealthController(TransactionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Health()
    {
        try
        {
            await _dbContext.Database.CanConnectAsync();
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
        catch
        {
            return StatusCode(503, new { status = "unhealthy", timestamp = DateTime.UtcNow });
        }
    }

    [HttpGet("metrics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Metrics()
    {
        var transactionCount = await _dbContext.Transactions.CountAsync();
        var importCount = await _dbContext.Imports.CountAsync();
        
        return Ok(new
        {
            transactions_total = transactionCount,
            imports_total = importCount,
            timestamp = DateTime.UtcNow
        });
    }
}
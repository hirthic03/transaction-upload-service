using Microsoft.EntityFrameworkCore;
using Serilog;
using Transactions.Api.Middleware;
using Transactions.Domain.Parsers;
using Transactions.Domain.Repositories;
using Transactions.Domain.Services;
using Transactions.Infrastructure.Data;
using Transactions.Infrastructure.Repositories;
using FluentValidation;
using Transactions.Domain.Validators;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ApplicationName", "Transactions.Api")
    .WriteTo.Console()
    .WriteTo.File("logs/transactions-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Transactions API", 
        Version = "v1",
        Description = "API for uploading and querying transaction data"
    });
});

// Configure DbContext
builder.Services.AddDbContext<TransactionDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IImportRepository, ImportRepository>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IFileParser, CsvParser>();
builder.Services.AddScoped<IFileParser, XmlParser>();
builder.Services.AddScoped<IValidator<Transactions.Domain.Models.TransactionRecord>, TransactionValidator>();

// Configure CORS
// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // In development, allow any origin (including file://)
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            // In production, restrict to specific origins
            policy.WithOrigins("http://localhost:3000", "http://localhost:5000")
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
    });
});

var app = builder.Build();

// Configure middleware pipeline
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin");
app.UseAuthorization();
app.MapControllers();

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();
    dbContext.Database.Migrate();
}

app.Run();

public partial class Program { }
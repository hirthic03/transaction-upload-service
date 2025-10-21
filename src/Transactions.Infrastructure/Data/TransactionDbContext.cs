using Microsoft.EntityFrameworkCore;
using Transactions.Domain.Entities;
using Transactions.Infrastructure.Data.Configurations;

namespace Transactions.Infrastructure.Data;

public class TransactionDbContext : DbContext
{
    public TransactionDbContext(DbContextOptions<TransactionDbContext> options)
        : base(options)
    {
    }

    public DbSet<Transaction> Transactions { get; set; } = null!;
    public DbSet<Import> Imports { get; set; } = null!;
    public DbSet<ImportError> ImportErrors { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new TransactionConfiguration());
        modelBuilder.ApplyConfiguration(new ImportConfiguration());
        modelBuilder.ApplyConfiguration(new ImportErrorConfiguration());

        // Add indexes for query performance
        modelBuilder.Entity<Transaction>()
            .HasIndex(t => t.CurrencyCode)
            .HasDatabaseName("IX_Transactions_CurrencyCode");

        modelBuilder.Entity<Transaction>()
            .HasIndex(t => t.StatusCode)
            .HasDatabaseName("IX_Transactions_StatusCode");

        modelBuilder.Entity<Transaction>()
            .HasIndex(t => t.TransactionDate)
            .HasDatabaseName("IX_Transactions_TransactionDate");

        modelBuilder.Entity<Import>()
            .HasIndex(i => i.Sha256Hash)
            .IsUnique()
            .HasDatabaseName("IX_Imports_Sha256Hash");
    }
}
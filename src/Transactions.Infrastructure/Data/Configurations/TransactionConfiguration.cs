using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Transactions.Domain.Entities;

namespace Transactions.Infrastructure.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.Amount)
            .HasColumnName("amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(t => t.CurrencyCode)
            .HasColumnName("currency_code")
            .HasColumnType("char(3)")
            .IsRequired();

        builder.Property(t => t.TransactionDate)
            .HasColumnName("transaction_date")
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(t => t.StatusCode)
            .HasColumnName("status_code")
            .HasColumnType("char(1)")
            .IsRequired();

        builder.Property(t => t.SourceFormat)
            .HasColumnName("source_format")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(t => t.ImportId)
            .HasColumnName("import_id")
            .IsRequired();

        builder.HasOne(t => t.Import)
            .WithMany(i => i.Transactions)
            .HasForeignKey(t => t.ImportId);
    }
}
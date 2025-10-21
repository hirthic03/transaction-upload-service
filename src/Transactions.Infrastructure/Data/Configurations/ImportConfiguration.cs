using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Transactions.Domain.Entities;

namespace Transactions.Infrastructure.Data.Configurations;

public class ImportConfiguration : IEntityTypeConfiguration<Import>
{
    public void Configure(EntityTypeBuilder<Import> builder)
    {
        builder.ToTable("imports");

        builder.HasKey(i => i.ImportId);

        builder.Property(i => i.ImportId)
            .HasColumnName("import_id")
            .HasColumnType("uniqueidentifier")
            .IsRequired();

        builder.Property(i => i.ReceivedAt)
            .HasColumnName("received_at")
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(i => i.SourceFormat)
            .HasColumnName("source_format")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(i => i.Sha256Hash)
            .HasColumnName("sha256")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(i => i.RecordCount)
            .HasColumnName("record_count")
            .IsRequired();

        builder.Property(i => i.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .IsRequired();
    }
}
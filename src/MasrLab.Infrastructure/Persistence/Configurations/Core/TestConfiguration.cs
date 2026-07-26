using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Core;

namespace MasrLab.Infrastructure.Persistence.Configurations.Core;

public class TestConfiguration : IEntityTypeConfiguration<Test>
{
    public void Configure(EntityTypeBuilder<Test> builder)
    {
        builder.ToTable("Tests");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.ReportName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.ReceiptName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Group).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Barcode).HasMaxLength(200);
        builder.Property(e => e.Price).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(e => e.TurnaroundTime).HasMaxLength(100).IsRequired();
        builder.Property(e => e.LabToLabFlag).IsRequired();
        builder.Property(e => e.Unit).HasMaxLength(100).IsRequired();

        builder.HasIndex(e => e.Group);
        builder.HasIndex(e => e.Barcode);
        builder.HasIndex(e => e.IsDeleted);
    }
}

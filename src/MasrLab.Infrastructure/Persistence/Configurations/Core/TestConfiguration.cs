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

        builder.Property(e => e.TestCode).HasMaxLength(200);
        builder.Property(e => e.HistoryName).HasMaxLength(200);
        builder.Property(e => e.ArabicName).HasMaxLength(200);
        builder.Property(e => e.Branch).HasMaxLength(200);
        builder.Property(e => e.LogGroup).HasMaxLength(200);
        builder.Property(e => e.SampleType).HasMaxLength(200);
        builder.Property(e => e.SeeReport).IsRequired();
        builder.Property(e => e.PrintWithOther).IsRequired();
        builder.Property(e => e.AddWithGroup).IsRequired();
        builder.Property(e => e.IsMainTest).IsRequired();
        builder.Property(e => e.TestTimeDays).IsRequired();
        builder.Property(e => e.ArrangeNo).IsRequired();
        builder.Property(e => e.ReferenceType).IsRequired();
        builder.Property(e => e.LabToLabPrice).HasColumnType("decimal(18,2)");
        builder.Property(e => e.BarcodeName).HasMaxLength(200);
        builder.Property(e => e.Tube1).HasMaxLength(200);
        builder.Property(e => e.Tube2).HasMaxLength(200);
        builder.Property(e => e.Tube3).HasMaxLength(200);
        builder.Property(e => e.SentOutsideLab).IsRequired();
        builder.Property(e => e.OutsourcedLabName).HasMaxLength(200);
        builder.Property(e => e.OutsourcedCostPrice).HasColumnType("decimal(18,2)");
        builder.Property(e => e.CostPrice).HasColumnType("decimal(18,2)");
        builder.Property(e => e.PatientQuestion).HasMaxLength(2000);

        builder.HasIndex(e => e.Group);
        builder.HasIndex(e => e.Barcode);
        builder.HasIndex(e => e.IsDeleted);
        builder.HasIndex(e => e.ArrangeNo);

        builder.HasMany(e => e.TestComponents)
            .WithOne()
            .HasForeignKey(e => e.TestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

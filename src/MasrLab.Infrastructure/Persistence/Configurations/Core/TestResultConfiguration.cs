using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Core;

namespace MasrLab.Infrastructure.Persistence.Configurations.Core;

#pragma warning disable CS8602

public class TestResultConfiguration : IEntityTypeConfiguration<TestResult>
{
    public void Configure(EntityTypeBuilder<TestResult> builder)
    {
        builder.ToTable("TestResults");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.VisitTestResultItemId).IsRequired();
        builder.Property(e => e.Value).HasMaxLength(500).IsRequired();
        builder.Property(e => e.Unit).HasMaxLength(100).IsRequired();
        builder.Property(e => e.ReferenceRange).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Status).IsRequired();
        builder.Property(e => e.EnteredByUserId).IsRequired();
        builder.Property(e => e.EnteredAt).IsRequired();
        builder.Property(e => e.OverrideReason).HasMaxLength(500);
        builder.Property(e => e.PrintedByUserId);
        builder.Property(e => e.PrintedAt);
        builder.Property(e => e.PrintCount).IsRequired();
        builder.Property(e => e.Comment).HasMaxLength(1000);
        builder.Property(e => e.ReprintRequired).IsRequired();

        builder.HasOne<VisitTestResultItem>()
            .WithMany()
            .HasForeignKey(e => e.VisitTestResultItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.VisitTestResultItemId);
        builder.HasIndex(e => e.EnteredByUserId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.IsDeleted);

        builder.HasIndex(e => new { e.VisitTestResultItemId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}

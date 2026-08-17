using MasrLab.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MasrLab.Infrastructure.Persistence.Configurations.Core;

public class TestResultEditHistoryConfiguration : IEntityTypeConfiguration<TestResultEditHistory>
{
    public void Configure(EntityTypeBuilder<TestResultEditHistory> builder)
    {
        builder.ToTable("TestResultEditHistories");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.TestResultId).IsRequired();
        builder.Property(e => e.OldValue).HasMaxLength(500);
        builder.Property(e => e.NewValue).HasMaxLength(500);
        builder.Property(e => e.OldComment).HasMaxLength(1000);
        builder.Property(e => e.NewComment).HasMaxLength(1000);
        builder.Property(e => e.ChangeType).IsRequired();
        builder.Property(e => e.EditedByUserId).IsRequired();
        builder.Property(e => e.EditedAt).IsRequired();

        builder.HasOne<TestResult>()
            .WithMany()
            .HasForeignKey(e => e.TestResultId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.TestResultId);
        builder.HasIndex(e => e.EditedByUserId);
        builder.HasIndex(e => e.IsDeleted);
    }
}

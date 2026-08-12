using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Core;

namespace MasrLab.Infrastructure.Persistence.Configurations.Core;

public class ReferenceValueConfiguration : IEntityTypeConfiguration<ReferenceValue>
{
    public void Configure(EntityTypeBuilder<ReferenceValue> builder)
    {
        builder.ToTable("ReferenceValues");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.TestId).IsRequired();
        builder.Property(e => e.Gender).IsRequired();
        builder.Property(e => e.AgeMin).IsRequired();
        builder.Property(e => e.AgeMax).IsRequired();
        builder.Property(e => e.AgeUnit).IsRequired();
        builder.Property(e => e.NormalRange).HasMaxLength(200).IsRequired();
        builder.Property(e => e.LowLimit).HasColumnType("decimal(18,2)");
        builder.Property(e => e.HighLimit).HasColumnType("decimal(18,2)");
        builder.Property(e => e.TestUnit).HasMaxLength(50);
        builder.Property(e => e.LowFlag).HasMaxLength(50);
        builder.Property(e => e.HighFlag).HasMaxLength(50);
        builder.Property(e => e.ForPregnantOnly).IsRequired();
        builder.Property(e => e.HighComment).HasMaxLength(500);
        builder.Property(e => e.LowComment).HasMaxLength(500);

        builder.HasIndex(e => e.TestId);
        builder.HasIndex(e => e.IsDeleted);
    }
}

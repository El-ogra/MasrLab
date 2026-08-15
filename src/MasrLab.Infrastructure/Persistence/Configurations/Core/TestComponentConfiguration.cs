using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Core;

namespace MasrLab.Infrastructure.Persistence.Configurations.Core;

public class TestComponentConfiguration : IEntityTypeConfiguration<TestComponent>
{
    public void Configure(EntityTypeBuilder<TestComponent> builder)
    {
        builder.ToTable("TestComponents");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.TestId).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Unit).HasMaxLength(100).IsRequired();
        builder.Property(e => e.DisplayOrder).IsRequired();
        builder.Property(e => e.ResultEntryKind).IsRequired();

        builder.HasIndex(e => e.TestId);
        builder.HasIndex(e => e.IsDeleted);

        builder.HasIndex(e => new { e.TestId, e.Name })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(e => new { e.TestId, e.DisplayOrder })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}

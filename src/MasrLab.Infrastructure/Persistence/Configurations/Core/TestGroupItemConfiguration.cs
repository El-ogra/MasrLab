using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Core;

namespace MasrLab.Infrastructure.Persistence.Configurations.Core;

public class TestGroupItemConfiguration : IEntityTypeConfiguration<TestGroupItem>
{
    public void Configure(EntityTypeBuilder<TestGroupItem> builder)
    {
        builder.ToTable("TestGroupItems");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.TestGroupId).IsRequired();
        builder.Property(e => e.TestId).IsRequired();
        builder.Property(e => e.DisplayOrder).IsRequired();

        builder.HasIndex(e => e.TestGroupId);
        builder.HasIndex(e => e.TestId);
        builder.HasIndex(e => e.IsDeleted);
    }
}

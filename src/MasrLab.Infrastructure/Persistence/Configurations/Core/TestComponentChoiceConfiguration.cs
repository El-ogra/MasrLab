using MasrLab.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MasrLab.Infrastructure.Persistence.Configurations.Core;

public class TestComponentChoiceConfiguration : IEntityTypeConfiguration<TestComponentChoice>
{
    public void Configure(EntityTypeBuilder<TestComponentChoice> builder)
    {
        builder.ToTable("TestComponentChoices");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.TestComponentId).IsRequired();
        builder.Property(e => e.Value).HasMaxLength(500).IsRequired();
        builder.Property(e => e.DisplayOrder).IsRequired();
        builder.Property(e => e.IsActive).IsRequired();

        builder.HasOne<TestComponent>()
            .WithMany()
            .HasForeignKey(e => e.TestComponentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.TestComponentId, e.Value })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(e => e.TestComponentId);
        builder.HasIndex(e => e.IsDeleted);
    }
}

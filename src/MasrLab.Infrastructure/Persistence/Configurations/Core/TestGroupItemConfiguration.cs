using MasrLab.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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
        builder.Property(e => e.Price).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(e => e.DisplayOrder).IsRequired();

        builder.HasIndex(e => new { e.TestGroupId, e.TestId })
            .HasFilter("[IsDeleted] = 0")
            .IsUnique();

        builder.HasIndex(e => e.TestGroupId);
        builder.HasIndex(e => e.TestId);
        builder.HasIndex(e => e.IsDeleted);

        builder.HasOne<TestGroup>()
            .WithMany(e => e.TestGroupItems)
            .HasForeignKey(e => e.TestGroupId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasOne<Test>()
            .WithMany()
            .HasForeignKey(e => e.TestId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}

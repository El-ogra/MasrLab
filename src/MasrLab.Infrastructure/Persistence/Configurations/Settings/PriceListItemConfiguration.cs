using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MasrLab.Infrastructure.Persistence.Configurations.Settings;

public class PriceListItemConfiguration : IEntityTypeConfiguration<PriceListItem>
{
    public void Configure(EntityTypeBuilder<PriceListItem> builder)
    {
        builder.ToTable("PriceListItems");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.PriceListId).IsRequired();
        builder.Property(e => e.TestId).IsRequired();
        builder.Property(e => e.Price).HasColumnType("decimal(18,2)").IsRequired();

        builder.HasIndex(e => new { e.PriceListId, e.TestId }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(e => e.PriceListId);
        builder.HasIndex(e => e.TestId);
        builder.HasIndex(e => e.IsDeleted);

        builder.HasOne<PriceList>()
            .WithMany(e => e.PriceListItems)
            .HasForeignKey(e => e.PriceListId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasOne<Test>()
            .WithMany()
            .HasForeignKey(e => e.TestId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}

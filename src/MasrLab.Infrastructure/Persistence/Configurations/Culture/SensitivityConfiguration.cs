using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Culture;

namespace MasrLab.Infrastructure.Persistence.Configurations.Culture;

public class SensitivityConfiguration : IEntityTypeConfiguration<Sensitivity>
{
    public void Configure(EntityTypeBuilder<Sensitivity> builder)
    {
        builder.ToTable("Sensitivities");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.CultureId).IsRequired();
        builder.Property(e => e.AntibioticId).IsRequired();
        builder.Property(e => e.SensitivityLevel).IsRequired();
        builder.Property(e => e.OrganismSlot).IsRequired();
        builder.Property(e => e.InhibitionZoneOverride).HasMaxLength(100);

        builder.HasIndex(e => new { e.CultureId, e.OrganismSlot, e.AntibioticId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(e => e.CultureId);
        builder.HasIndex(e => e.AntibioticId);
        builder.HasIndex(e => e.IsDeleted);
    }
}

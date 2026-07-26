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

        builder.HasIndex(e => e.CultureId);
        builder.HasIndex(e => e.AntibioticId);
        builder.HasIndex(e => e.IsDeleted);
    }
}

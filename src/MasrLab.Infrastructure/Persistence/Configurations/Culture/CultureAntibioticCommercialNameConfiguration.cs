using MasrLab.Domain.Entities.Culture;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MasrLab.Infrastructure.Persistence.Configurations.Culture;

public sealed class CultureAntibioticCommercialNameConfiguration : IEntityTypeConfiguration<CultureAntibioticCommercialName>
{
    public void Configure(EntityTypeBuilder<CultureAntibioticCommercialName> builder)
    {
        builder.ToTable("CultureAntibioticCommercialNames");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.CultureAntibioticId).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Print).HasDefaultValue(false).IsRequired();

        builder.HasOne(e => e.CultureAntibiotic)
            .WithMany(e => e.CommercialNames)
            .HasForeignKey(e => e.CultureAntibioticId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.CultureAntibioticId);
        builder.HasIndex(e => e.IsDeleted);
    }
}

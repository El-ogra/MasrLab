using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Culture;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MasrLab.Infrastructure.Persistence.Configurations.Culture;

public sealed class CultureAntibioticConfiguration : IEntityTypeConfiguration<CultureAntibiotic>
{
    public void Configure(EntityTypeBuilder<CultureAntibiotic> builder)
    {
        builder.ToTable("CultureAntibiotics");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.CultureTestId).IsRequired();
        builder.Property(e => e.AntibioticId).IsRequired();
        builder.Property(e => e.SensitivityText).HasMaxLength(200);
        builder.Property(e => e.Pregnant).HasDefaultValue(false).IsRequired();
        builder.Property(e => e.Children).HasDefaultValue(false).IsRequired();

        builder.HasOne<Test>()
            .WithMany()
            .HasForeignKey(e => e.CultureTestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Antibiotic)
            .WithMany()
            .HasForeignKey(e => e.AntibioticId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.CommercialNames)
            .WithOne(e => e.CultureAntibiotic)
            .HasForeignKey(e => e.CultureAntibioticId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.CultureTestId);
        builder.HasIndex(e => e.AntibioticId);
        builder.HasIndex(e => e.IsDeleted);
        builder.HasIndex(e => new { e.CultureTestId, e.AntibioticId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}

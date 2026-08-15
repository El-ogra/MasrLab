using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Core;

namespace MasrLab.Infrastructure.Persistence.Configurations.Core;

public class VisitCommercialPackageConfiguration : IEntityTypeConfiguration<VisitCommercialPackage>
{
    public void Configure(EntityTypeBuilder<VisitCommercialPackage> builder)
    {
        builder.ToTable("VisitCommercialPackages");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.PatientVisitId).IsRequired();
        builder.Property(e => e.CommercialPackageId).IsRequired();
        builder.Property(e => e.PackageNameSnapshot).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Price).HasColumnType("decimal(18,2)").IsRequired();

        builder.HasIndex(e => e.PatientVisitId);
        builder.HasIndex(e => e.CommercialPackageId);
        builder.HasIndex(e => e.IsDeleted);
    }
}

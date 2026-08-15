using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Core;

namespace MasrLab.Infrastructure.Persistence.Configurations.Core;

public class CommercialPackageConfiguration : IEntityTypeConfiguration<CommercialPackage>
{
    public void Configure(EntityTypeBuilder<CommercialPackage> builder)
    {
        builder.ToTable("CommercialPackages");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.IsActive).IsRequired();

        builder.HasIndex(e => e.Name);
        builder.HasIndex(e => e.IsDeleted);
    }
}

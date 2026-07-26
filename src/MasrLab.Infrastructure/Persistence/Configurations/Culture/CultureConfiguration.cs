using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Culture;

namespace MasrLab.Infrastructure.Persistence.Configurations.Culture;

public class CultureConfiguration : IEntityTypeConfiguration<Domain.Entities.Culture.Culture>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Culture.Culture> builder)
    {
        builder.ToTable("Cultures");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.SampleType).HasMaxLength(200).IsRequired();
        builder.Property(e => e.OrganismA).HasMaxLength(200);
        builder.Property(e => e.OrganismB).HasMaxLength(200);
        builder.Property(e => e.OrganismC).HasMaxLength(200);
        builder.Property(e => e.CultureCondition).HasMaxLength(200).IsRequired();
        builder.Property(e => e.ColonyCount).IsRequired();

        builder.HasIndex(e => e.IsDeleted);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Culture;

namespace MasrLab.Infrastructure.Persistence.Configurations.Culture;

public class AntibioticConfiguration : IEntityTypeConfiguration<Antibiotic>
{
    public void Configure(EntityTypeBuilder<Antibiotic> builder)
    {
        builder.ToTable("Antibiotics");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.ScientificName).HasMaxLength(300).IsRequired();

        builder.HasIndex(e => e.Name);
        builder.HasIndex(e => e.IsDeleted);
    }
}

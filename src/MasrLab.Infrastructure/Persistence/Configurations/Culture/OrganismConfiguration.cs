using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Culture;

namespace MasrLab.Infrastructure.Persistence.Configurations.Culture;

public class OrganismConfiguration : IEntityTypeConfiguration<Organism>
{
    public void Configure(EntityTypeBuilder<Organism> builder)
    {
        builder.ToTable("Organisms");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();

        builder.HasIndex(e => e.Name);
        builder.HasIndex(e => e.IsDeleted);
    }
}

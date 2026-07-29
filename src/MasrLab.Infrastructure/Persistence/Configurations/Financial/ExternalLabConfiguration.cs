using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Financial;

namespace MasrLab.Infrastructure.Persistence.Configurations.Financial;

public class ExternalLabConfiguration : IEntityTypeConfiguration<ExternalLab>
{
    public void Configure(EntityTypeBuilder<ExternalLab> builder)
    {
        builder.ToTable("ExternalLabs");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Address).HasMaxLength(200);
        builder.Property(e => e.Phone).HasMaxLength(20);
        builder.Property(e => e.ContactPerson).HasMaxLength(100);

        builder.HasIndex(e => e.Name);
        builder.HasIndex(e => e.IsDeleted);
    }
}

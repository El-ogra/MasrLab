using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Core;

namespace MasrLab.Infrastructure.Persistence.Configurations.Core;

public class SampleCollectionConfiguration : IEntityTypeConfiguration<SampleCollection>
{
    public void Configure(EntityTypeBuilder<SampleCollection> builder)
    {
        builder.ToTable("SampleCollections");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.SampleId).IsRequired();
        builder.Property(e => e.PatientId).IsRequired();
        builder.Property(e => e.IsCollected).IsRequired();
        builder.Property(e => e.CollectedAt).IsRequired();

        builder.HasIndex(e => e.SampleId);
        builder.HasIndex(e => e.PatientId);
        builder.HasIndex(e => e.IsDeleted);
    }
}

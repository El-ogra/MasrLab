using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Core;

namespace MasrLab.Infrastructure.Persistence.Configurations.Core;

public class SampleConfiguration : IEntityTypeConfiguration<Sample>
{
    public void Configure(EntityTypeBuilder<Sample> builder)
    {
        builder.ToTable("Samples");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.PatientVisitId).IsRequired();
        builder.Property(e => e.TestId).IsRequired();
        builder.Property(e => e.SampleType).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Barcode).HasMaxLength(200);
        builder.Property(e => e.CollectionStatus).IsRequired();

        builder.HasIndex(e => e.PatientVisitId);
        builder.HasIndex(e => e.TestId);
        builder.HasIndex(e => e.Barcode);
        builder.HasIndex(e => e.IsDeleted);
    }
}

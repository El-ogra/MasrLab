using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Core;

namespace MasrLab.Infrastructure.Persistence.Configurations.Core;

public class VisitTestConfiguration : IEntityTypeConfiguration<VisitTest>
{
    public void Configure(EntityTypeBuilder<VisitTest> builder)
    {
        builder.ToTable("VisitTests");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.PatientVisitId).IsRequired();
        builder.Property(e => e.TestId).IsRequired();
        builder.Property(e => e.Price).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(e => e.IsOutsourced).IsRequired();
        builder.Property(e => e.ExternalLabId);
        builder.Property(e => e.CostPrice).HasColumnType("decimal(18,2)");
        builder.Property(e => e.Notes).HasMaxLength(500);

        builder.HasIndex(e => e.PatientVisitId);
        builder.HasIndex(e => e.TestId);
        builder.HasIndex(e => e.IsDeleted);
    }
}

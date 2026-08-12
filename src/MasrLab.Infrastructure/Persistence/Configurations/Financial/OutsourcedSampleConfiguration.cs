using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Financial;

namespace MasrLab.Infrastructure.Persistence.Configurations.Financial;

public class OutsourcedSampleConfiguration : IEntityTypeConfiguration<OutsourcedSample>
{
    public void Configure(EntityTypeBuilder<OutsourcedSample> builder)
    {
        builder.ToTable("OutsourcedSamples");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.PatientVisitId).IsRequired();
        builder.Property(e => e.TestId).IsRequired();
        builder.Property(e => e.ExternalLabId).IsRequired();
        builder.Property(e => e.CostPrice).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(e => e.PatientPrice).HasColumnType("decimal(18,2)");
        builder.Property(e => e.SettlementStatus).IsRequired();
        builder.Property(e => e.ReceivedAt);

        builder.HasIndex(e => e.PatientVisitId);
        builder.HasIndex(e => e.TestId);
        builder.HasIndex(e => e.ExternalLabId);
        builder.HasIndex(e => e.IsDeleted);
    }
}
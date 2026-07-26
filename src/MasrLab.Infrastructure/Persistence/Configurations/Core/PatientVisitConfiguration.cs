using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Core;

namespace MasrLab.Infrastructure.Persistence.Configurations.Core;

public class PatientVisitConfiguration : IEntityTypeConfiguration<PatientVisit>
{
    public void Configure(EntityTypeBuilder<PatientVisit> builder)
    {
        builder.ToTable("PatientVisits");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.VisitDate).IsRequired();
        builder.Property(e => e.PatientId).IsRequired();
        builder.Property(e => e.Status).IsRequired();
        builder.Property(e => e.RegisteredByUserId).IsRequired();
        builder.Property(e => e.LabId).HasMaxLength(100).IsRequired();
        builder.Property(e => e.SampleStatus).IsRequired();
        builder.Property(e => e.TakenOutsideLab).IsRequired();

        builder.HasIndex(e => e.PatientId);
        builder.HasIndex(e => e.LabId);
        builder.HasIndex(e => e.VisitDate);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.RegisteredByUserId);
        builder.HasIndex(e => e.IsDeleted);
    }
}

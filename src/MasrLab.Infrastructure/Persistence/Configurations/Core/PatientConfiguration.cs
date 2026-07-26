using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Core;

namespace MasrLab.Infrastructure.Persistence.Configurations.Core;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Phone).HasMaxLength(50);
        builder.Property(e => e.Address).HasMaxLength(500);
        builder.Property(e => e.NationalId).HasMaxLength(100);
        builder.Property(e => e.Notes).HasMaxLength(1000);
        builder.Property(e => e.LabId).HasMaxLength(100).IsRequired();
        builder.Property(e => e.DrugAllergy).HasMaxLength(500);
        builder.Property(e => e.ChronicDiseases).HasMaxLength(500);

        builder.Property(e => e.AgeYears).IsRequired();
        builder.Property(e => e.AgeMonths).IsRequired();
        builder.Property(e => e.AgeDays).IsRequired();
        builder.Property(e => e.Gender).IsRequired();
        builder.Property(e => e.DoctorId).IsRequired();
        builder.Property(e => e.ReferralEntityId).IsRequired();
        builder.Property(e => e.AccountType).IsRequired();
        builder.Property(e => e.Pregnancy).IsRequired();
        builder.Property(e => e.BloodThinning).IsRequired();

        builder.HasIndex(e => e.LabId);
        builder.HasIndex(e => e.DoctorId);
        builder.HasIndex(e => e.NationalId);
        builder.HasIndex(e => e.IsDeleted);
    }
}

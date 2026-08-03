using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.ValueObjects;

namespace MasrLab.Infrastructure.Persistence.Configurations.Core;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Address).HasMaxLength(500);
        builder.Property(e => e.NationalId).HasMaxLength(100);
        builder.Property(e => e.Notes).HasMaxLength(1000);
        builder.Property(e => e.LabId).HasMaxLength(100).IsRequired();
        builder.Property(e => e.DrugAllergy).HasMaxLength(500);
        builder.Property(e => e.ChronicDiseases).HasMaxLength(500);

        builder.OwnsOne(e => e.Age, age =>
        {
            age.Property(a => a.Years).HasColumnName("AgeYears").IsRequired();
            age.Property(a => a.Months).HasColumnName("AgeMonths").IsRequired();
            age.Property(a => a.Days).HasColumnName("AgeDays").IsRequired();
        });

        builder.OwnsOne(e => e.Phone, phone =>
        {
            phone.Property(p => p.Value).HasColumnName("Phone").HasMaxLength(50);
        });

        builder.Property(e => e.Gender).IsRequired();
        builder.Property(e => e.DoctorId);
        builder.Property(e => e.ReferralEntityId);
        builder.Property(e => e.AccountType).IsRequired();
        builder.Property(e => e.Pregnancy).IsRequired();
        builder.Property(e => e.BloodThinning).IsRequired();
        builder.Property(e => e.HasDiabetes).IsRequired();
        builder.Property(e => e.HasHypertension).IsRequired();
        builder.Property(e => e.HasLiverDisease).IsRequired();
        builder.Property(e => e.HasJointDisease).IsRequired();
        builder.Property(e => e.HasRenalFailure).IsRequired();
        builder.Property(e => e.HasLupus).IsRequired();
        builder.Property(e => e.HasHeartDisease).IsRequired();
        builder.Property(e => e.HasThyroidDisorder).IsRequired();

        builder.HasIndex(e => e.LabId);
        builder.HasIndex(e => e.DoctorId);
        builder.HasIndex(e => e.NationalId);
        builder.HasIndex(e => e.IsDeleted);
    }
}

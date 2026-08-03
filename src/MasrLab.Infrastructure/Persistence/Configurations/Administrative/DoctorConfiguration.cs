using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.ValueObjects;

namespace MasrLab.Infrastructure.Persistence.Configurations.Administrative;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.ToTable("Doctors");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.OwnsOne(e => e.Phone, phone =>
        {
            phone.Property(p => p.Value).HasColumnName("Phone").HasMaxLength(20);
        });

        builder.Property(e => e.Address)
            .HasMaxLength(200);

        builder.Property(e => e.CommissionPercent)
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        builder.HasIndex(e => e.Name);
        builder.HasIndex(e => e.IsDeleted);
    }
}

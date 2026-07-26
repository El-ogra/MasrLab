using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Administrative;

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

        builder.Property(e => e.Phone)
            .HasMaxLength(20);

        builder.Property(e => e.Address)
            .HasMaxLength(200);

        builder.Property(e => e.CommissionPercent)
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        builder.HasIndex(e => e.Name);
    }
}

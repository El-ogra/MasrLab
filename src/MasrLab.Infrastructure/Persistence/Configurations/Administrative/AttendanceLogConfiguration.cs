using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.ValueObjects;

namespace MasrLab.Infrastructure.Persistence.Configurations.Administrative;

public class AttendanceLogConfiguration : IEntityTypeConfiguration<AttendanceLog>
{
    public void Configure(EntityTypeBuilder<AttendanceLog> builder)
    {
        builder.ToTable("AttendanceLogs");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId)
            .IsRequired();

        builder.OwnsOne(e => e.WorkPeriod, wp =>
        {
            wp.Property(p => p.Start).HasColumnName("LoginTime").IsRequired();
            wp.Property(p => p.End).HasColumnName("LogoutTime");
        });

        builder.Property(e => e.BreakPeriods)
            .HasMaxLength(500);

        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.IsDeleted);
    }
}

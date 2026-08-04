using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Administrative;

namespace MasrLab.Infrastructure.Persistence.Configurations.Administrative;

public class RequestAuditLogConfiguration : IEntityTypeConfiguration<RequestAuditLog>
{
    public void Configure(EntityTypeBuilder<RequestAuditLog> builder)
    {
        builder.ToTable("RequestAuditLogs");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.RequestName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.UserId)
            .IsRequired(false);

        builder.Property(e => e.ActionTimeUtc)
            .IsRequired();

        builder.Property(e => e.Success)
            .IsRequired();

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(2048);

        builder.HasIndex(e => e.RequestName);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.ActionTimeUtc);
        builder.HasIndex(e => e.IsDeleted);
    }
}

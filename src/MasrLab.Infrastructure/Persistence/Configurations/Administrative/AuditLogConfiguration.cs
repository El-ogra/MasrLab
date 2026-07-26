using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Administrative;

namespace MasrLab.Infrastructure.Persistence.Configurations.Administrative;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId)
            .IsRequired();

        builder.Property(e => e.ActionType)
            .IsRequired();

        builder.Property(e => e.EntityType)
            .IsRequired();

        builder.Property(e => e.EntityId)
            .IsRequired();

        builder.Property(e => e.ActionTime)
            .IsRequired();

        builder.Property(e => e.PrintCount)
            .IsRequired();

        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.ActionTime);
        builder.HasIndex(e => new { e.EntityType, e.EntityId });
    }
}

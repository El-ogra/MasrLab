using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Administrative;

namespace MasrLab.Infrastructure.Persistence.Configurations.Administrative;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId)
            .IsRequired();

        builder.Property(e => e.ScreenId)
            .IsRequired();

        builder.Property(e => e.OperationId)
            .IsRequired();

        builder.Property(e => e.Allowed)
            .IsRequired();

        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => new { e.UserId, e.ScreenId, e.OperationId })
            .IsUnique();
        builder.HasIndex(e => e.IsDeleted);
    }
}

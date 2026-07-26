using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Administrative;

namespace MasrLab.Infrastructure.Persistence.Configurations.Administrative;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Username)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Password)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.IsAdmin)
            .IsRequired();

        builder.Property(e => e.IsActive)
            .IsRequired();

        builder.HasIndex(e => e.Username)
            .IsUnique();

        builder.HasIndex(e => e.IsActive);
    }
}

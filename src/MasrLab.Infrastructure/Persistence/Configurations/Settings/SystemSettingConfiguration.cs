using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Settings;

namespace MasrLab.Infrastructure.Persistence.Configurations.Settings;

public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.ToTable("SystemSettings");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.SettingKey).HasMaxLength(200).IsRequired();
        builder.Property(e => e.SettingValue).HasMaxLength(1000).IsRequired();

        builder.HasIndex(e => e.SettingKey).IsUnique();
        builder.HasIndex(e => e.IsDeleted);
    }
}

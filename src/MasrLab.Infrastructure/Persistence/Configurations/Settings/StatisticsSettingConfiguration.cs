using MasrLab.Domain.Entities.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MasrLab.Infrastructure.Persistence.Configurations.Settings;

public class StatisticsSettingConfiguration : IEntityTypeConfiguration<StatisticsSetting>
{
    public void Configure(EntityTypeBuilder<StatisticsSetting> builder)
    {
        builder.ToTable("StatisticsSettings", table =>
        {
            table.HasCheckConstraint("CK_StatisticsSettings_DecimalPlaces", "[DecimalPlaces] BETWEEN 0 AND 4");
            table.HasCheckConstraint("CK_StatisticsSettings_DisplayOrder", "[DisplayOrder] > 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.KpiCode).HasConversion<byte>().IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.DisplayOrder).IsRequired();
        builder.Property(x => x.ValueFormat).HasConversion<byte>().IsRequired();
        builder.Property(x => x.ComparisonDirection).HasConversion<byte>().IsRequired();
        builder.Property(x => x.TargetValue).HasPrecision(18, 2);
        builder.Property(x => x.WarningThreshold).HasPrecision(18, 2);
        builder.Property(x => x.CriticalThreshold).HasPrecision(18, 2);
        builder.HasIndex(x => x.KpiCode).IsUnique();
        builder.HasIndex(x => new { x.IsEnabled, x.DisplayOrder });
        builder.HasIndex(x => x.IsDeleted);
    }
}

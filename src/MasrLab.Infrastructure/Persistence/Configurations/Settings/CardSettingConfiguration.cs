using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Settings;

namespace MasrLab.Infrastructure.Persistence.Configurations.Settings;

public class CardSettingConfiguration : IEntityTypeConfiguration<CardSetting>
{
    public void Configure(EntityTypeBuilder<CardSetting> builder)
    {
        builder.ToTable("CardSettings");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.CardTitle).HasMaxLength(200).IsRequired();
        builder.Property(e => e.ShowPatientName).IsRequired();
        builder.Property(e => e.ShowLabId).IsRequired();
        builder.Property(e => e.ShowNationalId).IsRequired();
        builder.Property(e => e.ShowPhone).IsRequired();
        builder.Property(e => e.ShowAge).IsRequired();
        builder.Property(e => e.ShowGender).IsRequired();
        builder.Property(e => e.ShowAddress).IsRequired();
        builder.Property(e => e.HeaderColor).HasMaxLength(50);
        builder.Property(e => e.FontSize).HasMaxLength(50);

        builder.HasIndex(e => e.IsDeleted);
    }
}

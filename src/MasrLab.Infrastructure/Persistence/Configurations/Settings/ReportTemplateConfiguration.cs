using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Settings;

namespace MasrLab.Infrastructure.Persistence.Configurations.Settings;

public class ReportTemplateConfiguration : IEntityTypeConfiguration<ReportTemplate>
{
    public void Configure(EntityTypeBuilder<ReportTemplate> builder)
    {
        builder.ToTable("ReportTemplates");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Margins).HasMaxLength(100).IsRequired();
        builder.Property(e => e.PaperSize).IsRequired();
        builder.Property(e => e.HeaderImage).HasMaxLength(500);
        builder.Property(e => e.HeaderText).HasMaxLength(500);
        builder.Property(e => e.FooterText).HasMaxLength(500);
        builder.Property(e => e.HeaderColor).HasMaxLength(50);
        builder.Property(e => e.FooterColor).HasMaxLength(50);

        builder.HasIndex(e => e.IsDeleted);
    }
}

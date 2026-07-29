using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Administrative;

namespace MasrLab.Infrastructure.Persistence.Configurations.Administrative;

public class CommentTemplateConfiguration : IEntityTypeConfiguration<CommentTemplate>
{
    public void Configure(EntityTypeBuilder<CommentTemplate> builder)
    {
        builder.ToTable("CommentTemplates");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.TestId).IsRequired();
        builder.Property(e => e.Text).HasMaxLength(2000).IsRequired();

        builder.HasIndex(e => e.TestId);
        builder.HasIndex(e => e.IsDeleted);
    }
}

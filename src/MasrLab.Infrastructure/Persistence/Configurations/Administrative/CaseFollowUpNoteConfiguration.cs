using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MasrLab.Infrastructure.Persistence.Configurations.Administrative;

public class CaseFollowUpNoteConfiguration : IEntityTypeConfiguration<CaseFollowUpNote>
{
    public void Configure(EntityTypeBuilder<CaseFollowUpNote> builder)
    {
        builder.ToTable("CaseFollowUpNotes");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.TestId).IsRequired();
        builder.Property(e => e.Notes).HasMaxLength(2000).IsRequired();

        builder.HasIndex(e => e.TestId);
        builder.HasIndex(e => e.IsDeleted);
        builder.HasOne<Test>()
            .WithMany()
            .HasForeignKey(e => e.TestId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

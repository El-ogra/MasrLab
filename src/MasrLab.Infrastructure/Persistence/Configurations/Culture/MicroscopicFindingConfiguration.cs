using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Culture;

namespace MasrLab.Infrastructure.Persistence.Configurations.Culture;

public class MicroscopicFindingConfiguration : IEntityTypeConfiguration<MicroscopicFinding>
{
    public void Configure(EntityTypeBuilder<MicroscopicFinding> builder)
    {
        builder.ToTable("MicroscopicFindings");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.CultureId).IsRequired();
        builder.Property(e => e.RowKey).IsRequired();
        builder.Property(e => e.Value).HasMaxLength(200).IsRequired();
        builder.Property(e => e.ReferenceRange).HasMaxLength(100);
        builder.Property(e => e.IncludeInPrint).IsRequired();

        // One row per block key per culture.
        builder.HasIndex(e => new { e.CultureId, e.RowKey })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(e => e.CultureId);
        builder.HasIndex(e => e.IsDeleted);

        builder.HasOne<Domain.Entities.Culture.Culture>()
            .WithMany(c => c.MicroscopicFindings)
            .HasForeignKey(e => e.CultureId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

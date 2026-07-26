using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Settings;

namespace MasrLab.Infrastructure.Persistence.Configurations.Settings;

public class WorkSheetConfiguration : IEntityTypeConfiguration<WorkSheet>
{
    public void Configure(EntityTypeBuilder<WorkSheet> builder)
    {
    }
}

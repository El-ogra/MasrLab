using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Culture;

namespace MasrLab.Infrastructure.Persistence.Configurations.Culture;

public class SensitivityConfiguration : IEntityTypeConfiguration<Sensitivity>
{
    public void Configure(EntityTypeBuilder<Sensitivity> builder)
    {
    }
}

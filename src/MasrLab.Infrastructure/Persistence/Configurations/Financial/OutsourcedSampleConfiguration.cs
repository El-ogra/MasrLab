using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Financial;

namespace MasrLab.Infrastructure.Persistence.Configurations.Financial;

public class OutsourcedSampleConfiguration : IEntityTypeConfiguration<OutsourcedSample>
{
    public void Configure(EntityTypeBuilder<OutsourcedSample> builder)
    {
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Core;

namespace MasrLab.Infrastructure.Persistence.Configurations.Core;

public class SampleCollectionConfiguration : IEntityTypeConfiguration<SampleCollection>
{
    public void Configure(EntityTypeBuilder<SampleCollection> builder)
    {
    }
}

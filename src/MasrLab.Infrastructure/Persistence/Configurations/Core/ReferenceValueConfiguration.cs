using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Core;

namespace MasrLab.Infrastructure.Persistence.Configurations.Core;

public class ReferenceValueConfiguration : IEntityTypeConfiguration<ReferenceValue>
{
    public void Configure(EntityTypeBuilder<ReferenceValue> builder)
    {
    }
}

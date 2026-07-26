using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Culture;

namespace MasrLab.Infrastructure.Persistence.Configurations.Culture;

public class CultureConfiguration : IEntityTypeConfiguration<Domain.Entities.Culture.Culture>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Culture.Culture> builder)
    {
    }
}

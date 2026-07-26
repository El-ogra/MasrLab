using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Culture;

namespace MasrLab.Infrastructure.Persistence.Configurations.Culture;

public class OrganismConfiguration : IEntityTypeConfiguration<Organism>
{
    public void Configure(EntityTypeBuilder<Organism> builder)
    {
    }
}

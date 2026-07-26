using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Culture;

namespace MasrLab.Infrastructure.Persistence.Configurations.Culture;

public class AntibioticConfiguration : IEntityTypeConfiguration<Antibiotic>
{
    public void Configure(EntityTypeBuilder<Antibiotic> builder)
    {
    }
}

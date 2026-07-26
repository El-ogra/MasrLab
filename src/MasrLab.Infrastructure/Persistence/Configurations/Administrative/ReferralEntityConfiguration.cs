using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Administrative;

namespace MasrLab.Infrastructure.Persistence.Configurations.Administrative;

public class ReferralEntityConfiguration : IEntityTypeConfiguration<ReferralEntity>
{
    public void Configure(EntityTypeBuilder<ReferralEntity> builder)
    {
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Core;

namespace MasrLab.Infrastructure.Persistence.Configurations.Core;

public class TestGroupConfiguration : IEntityTypeConfiguration<TestGroup>
{
    public void Configure(EntityTypeBuilder<TestGroup> builder)
    {
    }
}

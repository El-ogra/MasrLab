using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Core;

namespace MasrLab.Infrastructure.Persistence.Configurations.Core;

public class TestGroupItemConfiguration : IEntityTypeConfiguration<TestGroupItem>
{
    public void Configure(EntityTypeBuilder<TestGroupItem> builder)
    {
    }
}

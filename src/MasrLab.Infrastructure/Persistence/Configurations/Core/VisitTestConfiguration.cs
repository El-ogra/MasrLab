using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Core;

namespace MasrLab.Infrastructure.Persistence.Configurations.Core;

public class VisitTestConfiguration : IEntityTypeConfiguration<VisitTest>
{
    public void Configure(EntityTypeBuilder<VisitTest> builder)
    {
    }
}

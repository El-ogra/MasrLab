using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Core;

namespace MasrLab.Infrastructure.Persistence.Configurations.Core;

public class PatientVisitConfiguration : IEntityTypeConfiguration<PatientVisit>
{
    public void Configure(EntityTypeBuilder<PatientVisit> builder)
    {
    }
}

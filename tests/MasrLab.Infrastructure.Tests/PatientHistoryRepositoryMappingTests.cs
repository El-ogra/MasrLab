using MasrLab.Infrastructure.Persistence.Repositories;
using MasrLab.Infrastructure.Persistence.Views;

namespace MasrLab.Infrastructure.Tests;

public sealed class PatientHistoryRepositoryMappingTests
{
    [Fact]
    public void MapToEntry_ConvertsNullStatusesToEmptyStrings()
    {
        var entry = PatientHistoryRepository.MapToEntry(new PatientHistoryView
        {
            PreviousStatus = null,
            CurrentStatus = null
        });

        Assert.Equal(string.Empty, entry.PreviousStatus);
        Assert.Equal(string.Empty, entry.CurrentStatus);
    }
}

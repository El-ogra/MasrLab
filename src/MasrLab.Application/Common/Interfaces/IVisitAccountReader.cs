using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Common.Interfaces;

public interface IVisitAccountReader
{
    Task<VisitAccountDto?> GetAsync(int patientVisitId, CancellationToken cancellationToken = default);
}

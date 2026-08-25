using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Features.ResultsEntry.Queries.GetResultWorklist;

// M4-BR-05: one row per test — Abbreviation, Result-or-"See Report", Status,
// Finish, Verify, Print, Export columns.
public sealed record WorklistPatientDto(
    int VisitId,
    int PatientId,
    string PatientName,
    string LabId,
    AccountType AccountType,
    IReadOnlyList<WorklistTestRowDto> Tests);

public sealed record WorklistTestRowDto(
    int VisitTestId,
    int TestId,
    string Abbreviation,
    string ResultOrSeeReport,
    string? Status,
    bool IsFinished,
    bool IsVerified,
    bool IsPrinted,
    bool IsExportMarked);

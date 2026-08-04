using MediatR;

namespace MasrLab.Application.Features.CasesFollowUp.Commands.AddCaseFollowUp;

public record AddCaseFollowUpCommand(int TestId, string Notes) : IRequest<Unit>;

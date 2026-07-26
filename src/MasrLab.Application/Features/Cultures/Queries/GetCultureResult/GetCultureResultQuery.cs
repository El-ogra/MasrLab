using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.Cultures.Queries.GetCultureResult;

public record GetCultureResultQuery : IRequest<CultureResultDto?>;

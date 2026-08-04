using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.SystemSettings.Queries.GetSystemSettings;

public record GetSystemSettingsQuery : IRequest<SystemSettingsDto>;

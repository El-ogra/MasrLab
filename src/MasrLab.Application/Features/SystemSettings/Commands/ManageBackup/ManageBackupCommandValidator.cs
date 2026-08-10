using FluentValidation;
using MasrLab.Application.Common.Models;

namespace MasrLab.Application.Features.SystemSettings.Commands.ManageBackup;

public class ManageBackupCommandValidator : AbstractValidator<ManageBackupCommand>
{
    public ManageBackupCommandValidator()
    {
        RuleFor(x => x.Operation)
            .IsInEnum();

        RuleFor(x => x.FilePath)
            .NotEmpty();

        When(x => x.Operation == BackupOperation.Restore, () =>
        {
            RuleFor(x => x.RestoreConfirmation)
                .Equal("RESTORE");
        });
    }
}

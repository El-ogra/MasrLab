using FluentValidation;

namespace MasrLab.Application.Features.SystemSettings.Commands.ManageBackup;

public class ManageBackupCommandValidator : AbstractValidator<ManageBackupCommand>
{
    public ManageBackupCommandValidator()
    {
        RuleFor(x => x.BackupAction)
            .NotEmpty();

        RuleFor(x => x.FilePath)
            .NotEmpty();
    }
}

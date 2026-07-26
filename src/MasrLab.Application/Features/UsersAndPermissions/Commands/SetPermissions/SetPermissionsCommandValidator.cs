using FluentValidation;

namespace MasrLab.Application.Features.UsersAndPermissions.Commands.SetPermissions;

public class SetPermissionsCommandValidator : AbstractValidator<SetPermissionsCommand>
{
    public SetPermissionsCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.ScreenId).GreaterThan(0);
        RuleFor(x => x.OperationId).GreaterThan(0);
    }
}

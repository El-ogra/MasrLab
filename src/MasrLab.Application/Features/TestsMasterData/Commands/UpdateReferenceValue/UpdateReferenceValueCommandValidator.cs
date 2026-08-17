using FluentValidation;

namespace MasrLab.Application.Features.TestsMasterData.Commands.UpdateReferenceValue;

public class UpdateReferenceValueCommandValidator : AbstractValidator<UpdateReferenceValueCommand>
{
    public UpdateReferenceValueCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.TestId).GreaterThan(0);
        RuleFor(x => x.TestComponentId).GreaterThan(0);
        RuleFor(x => x.NormalRange).NotEmpty();
        RuleFor(x => x.AgeMin).GreaterThanOrEqualTo(0);
        RuleFor(x => x)
            .Must(x => (x.AgeMin == 0 && x.AgeMax == 0) || x.AgeMax > x.AgeMin)
            .WithMessage("يجب أن يكون أعلى عمر أكبر من أدنى عمر، أو كلاهما صفر للنطاق بدون قيد عمري");
    }
}

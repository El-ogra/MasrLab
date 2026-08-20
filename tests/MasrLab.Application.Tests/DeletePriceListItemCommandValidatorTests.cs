using MasrLab.Application.Features.PriceLists.Commands.DeletePriceListItem;

namespace MasrLab.Application.Tests;

public class DeletePriceListItemCommandValidatorTests
{
    private readonly DeletePriceListItemCommandValidator _validator = new();

    [Fact] public void ZeroId_Fails() => Assert.False(_validator.Validate(new DeletePriceListItemCommand(0)).IsValid);
    [Fact] public void PositiveId_Passes() => Assert.True(_validator.Validate(new DeletePriceListItemCommand(1)).IsValid);
}

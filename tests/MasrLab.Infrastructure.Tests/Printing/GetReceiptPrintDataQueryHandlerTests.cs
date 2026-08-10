using MasrLab.Application.Common.Printing;
using MasrLab.Application.Features.Printing.Queries.GetReceiptPrintData;
using Moq;

namespace MasrLab.Infrastructure.Tests.Printing;

public class GetReceiptPrintDataQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsThePayloadFromTheReader()
    {
        var payload = new ReceiptPrintDto { ReceiptId = 12, ReceiptNumber = "REC-000012" };
        var reader = new Mock<IReceiptPrintDataReader>();
        reader.Setup(x => x.GetAsync(12, It.IsAny<CancellationToken>())).ReturnsAsync(payload);

        var result = await new GetReceiptPrintDataQueryHandler(reader.Object)
            .Handle(new GetReceiptPrintDataQuery(12), CancellationToken.None);

        Assert.Same(payload, result);
    }

    [Fact]
    public async Task Handle_ThrowsWhenReceiptDoesNotExist()
    {
        var reader = new Mock<IReceiptPrintDataReader>();
        reader.Setup(x => x.GetAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((ReceiptPrintDto?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => new GetReceiptPrintDataQueryHandler(reader.Object)
            .Handle(new GetReceiptPrintDataQuery(99), CancellationToken.None));
    }
}

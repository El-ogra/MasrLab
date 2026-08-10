using MasrLab.Application.Common.Printing;
using MediatR;

namespace MasrLab.Application.Features.Printing.Queries.GetReceiptPrintData;

public sealed record GetReceiptPrintDataQuery(int ReceiptId) : IRequest<ReceiptPrintDto>;

public sealed class GetReceiptPrintDataQueryHandler : IRequestHandler<GetReceiptPrintDataQuery, ReceiptPrintDto>
{
    private readonly IReceiptPrintDataReader _reader;

    public GetReceiptPrintDataQueryHandler(IReceiptPrintDataReader reader) => _reader = reader;

    public async Task<ReceiptPrintDto> Handle(GetReceiptPrintDataQuery request, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(request.ReceiptId);
        return await _reader.GetAsync(request.ReceiptId, cancellationToken)
            ?? throw new KeyNotFoundException($"Receipt with id {request.ReceiptId} was not found.");
    }
}

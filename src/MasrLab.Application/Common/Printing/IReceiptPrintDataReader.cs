namespace MasrLab.Application.Common.Printing;

public interface IReceiptPrintDataReader
{
    Task<ReceiptPrintDto?> GetAsync(int receiptId, CancellationToken ct = default);
}

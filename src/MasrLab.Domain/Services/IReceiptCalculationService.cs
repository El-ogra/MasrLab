namespace MasrLab.Domain.Services;

public interface IReceiptCalculationService
{
    decimal CalculateRemaining(decimal total, decimal paid, decimal discount);
    decimal CalculateChangeDue(decimal paid, decimal total);
}

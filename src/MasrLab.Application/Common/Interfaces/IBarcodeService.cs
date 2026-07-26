namespace MasrLab.Application.Common.Interfaces;

public interface IBarcodeService
{
    byte[] GenerateBarcode(string content, int width = 300, int height = 100);
}

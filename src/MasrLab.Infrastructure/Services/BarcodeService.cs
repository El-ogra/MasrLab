using MasrLab.Application.Common.Interfaces;

namespace MasrLab.Infrastructure.Services;

public class BarcodeService : IBarcodeService
{
    public byte[] GenerateBarcode(string content, int width = 300, int height = 100)
    {
        throw new NotImplementedException();
    }
}

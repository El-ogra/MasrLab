using MasrLab.Infrastructure.Services;
using ZXing;
using ZXing.Common;

namespace MasrLab.Infrastructure.Tests;

public class BarcodeServiceTests
{
    [Fact]
    public void GenerateBarcode_EncodesAndDecodesTheOriginalLabId()
    {
        const string labId = "2026081000001";
        var png = new BarcodeService().GenerateBarcode(labId);

        Assert.NotEmpty(png);
        var decoded = new BarcodeReaderGeneric
        {
            Options = new DecodingOptions { PossibleFormats = [BarcodeFormat.CODE_128] }
        }.Decode(ToLuminanceSource(png));

        Assert.NotNull(decoded);
        Assert.Equal(labId, decoded.Text);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void GenerateBarcode_ThrowsForEmptyContent(string content)
    {
        Assert.Throws<ArgumentException>(() => new BarcodeService().GenerateBarcode(content));
    }

    [Fact]
    public void GenerateBarcode_ThrowsForNullContent()
    {
        Assert.Throws<ArgumentNullException>(() => new BarcodeService().GenerateBarcode(null!));
    }

    private static RGBLuminanceSource ToLuminanceSource(byte[] png)
    {
        var width = ReadInt32(png, 16);
        var height = ReadInt32(png, 20);
        using var compressed = new MemoryStream();

        for (var offset = 8; offset < png.Length;)
        {
            var length = ReadInt32(png, offset);
            var type = System.Text.Encoding.ASCII.GetString(png, offset + 4, 4);
            if (type == "IDAT")
                compressed.Write(png, offset + 8, length);

            offset += 12 + length;
        }

        compressed.Position = 0;
        using var decompressor = new System.IO.Compression.ZLibStream(compressed, System.IO.Compression.CompressionMode.Decompress);
        var rgba = new byte[width * height * 4];
        var scanline = new byte[width * 4 + 1];
        for (var row = 0; row < height; row++)
        {
            decompressor.ReadExactly(scanline);
            Assert.Equal(0, scanline[0]);
            Buffer.BlockCopy(scanline, 1, rgba, row * width * 4, width * 4);
        }

        return new RGBLuminanceSource(rgba, width, height, RGBLuminanceSource.BitmapFormat.RGBA32);
    }

    private static int ReadInt32(byte[] bytes, int offset) =>
        (bytes[offset] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3];
}

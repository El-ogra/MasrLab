using MasrLab.Application.Common.Interfaces;
using System.IO.Compression;
using ZXing;
using ZXing.Common;
using ZXing.Rendering;

namespace MasrLab.Infrastructure.Services;

public class BarcodeService : IBarcodeService
{
    public byte[] GenerateBarcode(string content, int width = 300, int height = 100)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(width, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(height, 0);

        var writer = new BarcodeWriterPixelData
        {
            Format = BarcodeFormat.CODE_128,
            Options = new EncodingOptions
            {
                Width = width,
                Height = height,
                Margin = 0,
                PureBarcode = false
            }
        };

        var pixelData = writer.Write(content);
        return PngEncoder.Encode(pixelData);
    }

    private static class PngEncoder
    {
        private static readonly byte[] Signature = [137, 80, 78, 71, 13, 10, 26, 10];

        public static byte[] Encode(PixelData pixelData)
        {
            using var stream = new MemoryStream();
            stream.Write(Signature);
            WriteChunk(stream, "IHDR", CreateHeader(pixelData));

            using var compressedPixels = new MemoryStream();
            using (var compressor = new ZLibStream(compressedPixels, CompressionLevel.Fastest, leaveOpen: true))
            {
                for (var y = 0; y < pixelData.Height; y++)
                {
                    compressor.WriteByte(0);
                    compressor.Write(pixelData.Pixels, y * pixelData.Width * 4, pixelData.Width * 4);
                }
            }

            WriteChunk(stream, "IDAT", compressedPixels.ToArray());
            WriteChunk(stream, "IEND", []);
            return stream.ToArray();
        }

        private static byte[] CreateHeader(PixelData pixelData)
        {
            var header = new byte[13];
            WriteInt32(header, 0, pixelData.Width);
            WriteInt32(header, 4, pixelData.Height);
            header[8] = 8;
            header[9] = 6;
            return header;
        }

        private static void WriteChunk(Stream stream, string type, byte[] data)
        {
            var typeBytes = System.Text.Encoding.ASCII.GetBytes(type);
            WriteInt32(stream, data.Length);
            stream.Write(typeBytes);
            stream.Write(data);

            WriteInt32(stream, unchecked((int)CalculateCrc32(typeBytes, data)));
        }

        private static void WriteInt32(Stream stream, int value)
        {
            stream.WriteByte((byte)(value >> 24));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)value);
        }

        private static void WriteInt32(byte[] bytes, int offset, int value)
        {
            bytes[offset] = (byte)(value >> 24);
            bytes[offset + 1] = (byte)(value >> 16);
            bytes[offset + 2] = (byte)(value >> 8);
            bytes[offset + 3] = (byte)value;
        }

        private static uint CalculateCrc32(byte[] first, byte[] second)
        {
            var crc = uint.MaxValue;
            foreach (var value in first.Concat(second))
            {
                crc ^= value;
                for (var bit = 0; bit < 8; bit++)
                    crc = (crc >> 1) ^ ((crc & 1) == 1 ? 0xEDB88320u : 0);
            }

            return ~crc;
        }
    }
}

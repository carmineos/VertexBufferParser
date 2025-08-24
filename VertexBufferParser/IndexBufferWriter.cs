using System.Buffers;

namespace VertexBufferParser;

public class IndexBufferWriter
{
    public const string DefaultIndicesSeparator = " ";
    public const int DefaultIndicesPerLine = 10;

    private readonly ElementDescriptor _elementDescriptor;
    private readonly IFormatProvider? _formatProvider;

    public string IndicesSeparator { get; set; } = DefaultIndicesSeparator;
    public int IndicesPerLine { get; set; } = DefaultIndicesPerLine;

    public IndexBufferWriter(ElementDescriptor elementDescriptor, IFormatProvider? formatProvider = null)
    {
        _elementDescriptor = elementDescriptor;
        _formatProvider = formatProvider;
    }

    public void Write(Span<byte> indexBuffer, TextWriter textWriter)
    {
        var pool = ArrayPool<char>.Shared;

        var elementSize = _elementDescriptor.GetElementSize();
        var indicesCount = indexBuffer.Length / elementSize;

        var maxLineLength = ComputeMaxLineLength(indicesCount);
        var lineBuffer = pool.Rent(maxLineLength);

        var elementWriter = GetIndexWriter(_elementDescriptor);

        var linesCount = (indicesCount / IndicesPerLine) + (indicesCount % IndicesPerLine == 0 ? 0 : 1);

        for (int i = 0; i < linesCount; i++)
        {
            Span<byte> indicesChunk;
            int charsWritten;

            if (i < linesCount - 1)
            {
                indicesChunk = indexBuffer.Slice(i * elementSize * IndicesPerLine, elementSize * IndicesPerLine);
                charsWritten = elementWriter.WriteElement(indicesChunk, lineBuffer, _formatProvider);

                // Copy the separator and increase the length
                Environment.NewLine.AsSpan().CopyTo(lineBuffer.AsSpan().Slice(charsWritten, Environment.NewLine.Length));
                charsWritten += Environment.NewLine.Length;
            }
            else
            {
                indicesChunk = indexBuffer.Slice(i * elementSize * IndicesPerLine);
                charsWritten = elementWriter.WriteElement(indicesChunk, lineBuffer, _formatProvider);
            }

            textWriter.Write(lineBuffer.AsSpan(0, charsWritten)); 
        }

        pool.Return(lineBuffer);
    }

    private int ComputeMaxLineLength(int count)
    {
        // Or even ushort.MaxValue/uint.MaxValue
        var maxDigitsCount = count.ToString().Length;

        return (IndicesPerLine * maxDigitsCount)
            + ((IndicesPerLine - 1) * IndicesSeparator.Length)
            + Environment.NewLine.Length;
    }

    public static IElementWriter GetIndexWriter(ElementDescriptor elementDescriptor, int? count = null, string? separator = null, string? format = null)
    {
        return elementDescriptor.Type switch
        {
            ElementDescriptorType.UShort => new ElementWriter<ushort>(count, separator, format),
            ElementDescriptorType.UInt => new ElementWriter<uint>(count, separator, format),
            _ => throw new Exception(),
        };
    }
}
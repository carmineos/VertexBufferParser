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
        
        // TODO: Compute buffer size
        var destination = pool.Rent(IndicesPerLine * 8);

        var writer = GetIndexWriter(_elementDescriptor);

        var linesCount = (indicesCount / IndicesPerLine) + (indicesCount % IndicesPerLine == 0 ? 0 : 1);

        for (int i = 0; i < linesCount; i++)
        {
            Span<byte> indicesChunk;
            int charsWritten;

            if (i < linesCount - 1)
            {
                indicesChunk = indexBuffer.Slice(i * elementSize * IndicesPerLine, elementSize * IndicesPerLine);
                charsWritten = writer.WriteElement(indicesChunk, destination, _formatProvider);

                // Copy the separator and increase the length
                Environment.NewLine.AsSpan().CopyTo(destination.AsSpan().Slice(charsWritten, Environment.NewLine.Length));
                charsWritten += Environment.NewLine.Length;
            }
            else
            {
                indicesChunk = indexBuffer.Slice(i * elementSize * IndicesPerLine);
                charsWritten = writer.WriteElement(indicesChunk, destination, _formatProvider);
            }

            textWriter.Write(destination.AsSpan(0, charsWritten));
            pool.Return(destination);
        }
    }

    public static IElementWriter GetIndexWriter(ElementDescriptor elementDescriptor, int? count = null, string? separator = null, string? format = null)
    {
        return elementDescriptor.Type switch
        {
            "UShort" => new ElementWriter<ushort>(count, separator, format),
            _ => throw new Exception(),
        };
    }
}
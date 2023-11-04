using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VertexBufferParser;

public interface IElementParser
{
    public (int,int) ParseElement(Span<byte> vertex, int vertexIndex, int itemsCount, ReadOnlySpan<char> line, int lineStart = 0, IFormatProvider? formatProvider = null);
}

public class ElementParser<T> : IElementParser where T : unmanaged, ISpanParsable<T>
{
    public static ElementParser<T> Singleton = new ElementParser<T>();

    public (int, int) ParseElement(Span<byte> vertex, int vertexOffsetStart, int componentsCount, ReadOnlySpan<char> line, int lineOffsetStart = 0, IFormatProvider? formatProvider = null)
    {
        int componentSize = Unsafe.SizeOf<T>();

        // Number of items already read
        int componentsRead = 0;

        // Index in the vertex being written
        int vertexOffset = vertexOffsetStart;

        // Index in the line being read
        int lineOffset = lineOffsetStart;

        formatProvider ??= NumberFormatInfo.InvariantInfo;

        int currentLineChunkStart = lineOffset;
        int currentLineChunkLength = 0;

        while (componentsRead < componentsCount && lineOffset < line.Length)
        {
            var c = line[lineOffset];

            // Found a space, Collect all the previous chars if any
            if (char.IsWhiteSpace(c))
            {
                if (currentLineChunkLength > 0)
                {
                    var lineChunk = line.Slice(currentLineChunkStart, currentLineChunkLength);
                    var vertexChunk = vertex.Slice(vertexOffset, componentSize);

                    ParseLineChunk(vertexChunk, lineChunk, formatProvider);
                    componentsRead++;
                    vertexOffset += componentSize;

                    currentLineChunkLength = 0;
                }

                currentLineChunkStart = lineOffset + 1;
            }
            else // More chars
            {
                currentLineChunkLength++;
            }

            lineOffset++;
        }

        // Collect last item
        if (currentLineChunkLength > 0)
        {
            var lineChunk = line.Slice(currentLineChunkStart, currentLineChunkLength);
            var vertexChunk = vertex.Slice(vertexOffset, componentSize);

            ParseLineChunk(vertexChunk, lineChunk, formatProvider);
            componentsRead++;
            vertexOffset += componentSize;

            currentLineChunkLength = 0;
        }

        return (vertexOffset, lineOffset);
    }

    public static void ParseLineChunk(Span<byte> vertexChunk, ReadOnlySpan<char> lineChunk, IFormatProvider? formatProvider = null)
    {
        T parsed = T.Parse(lineChunk, provider: formatProvider);
        Span<byte> bytes = MemoryMarshal.Cast<T, byte>(MemoryMarshal.CreateSpan(ref parsed, 1));
        bytes.CopyTo(vertexChunk);
    }
}
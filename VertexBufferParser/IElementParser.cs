using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VertexBufferParser;

public interface IElementParser
{
    public int Count { get; }

    public (int,int) ParseElement(Span<byte> vertex, int vertexIndex, ReadOnlySpan<char> line, int lineStart = 0, IFormatProvider? formatProvider = null);
}

public abstract class ElementParser<T> : IElementParser where T : unmanaged, ISpanParsable<T>
{
    public abstract int Count { get; }

    public virtual (int, int) ParseElement(Span<byte> vertex, int vertexOffsetStart, ReadOnlySpan<char> line, int lineOffsetStart = 0, IFormatProvider? formatProvider = null)
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

        while (componentsRead < Count && lineOffset < line.Length)
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

    public virtual void ParseLineChunk(Span<byte> vertexChunk, ReadOnlySpan<char> lineChunk, IFormatProvider? formatProvider = null)
    {
        T parsed = T.Parse(lineChunk, provider: formatProvider);
        Span<byte> bytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref parsed, 1));
        bytes.CopyTo(vertexChunk);
    }
}

public class Float2ElementParser : ElementParser<float>
{
    public static Float2ElementParser Singleton = new Float2ElementParser();

    public override int Count => 2;
}

public class Float3ElementParser : ElementParser<float>
{
    public static Float3ElementParser Singleton = new Float3ElementParser();

    public override int Count => 3;
}

public class ColourElementParser : ElementParser<byte>
{
    public static ColourElementParser Singleton = new ColourElementParser();

    public override int Count => 4;
}

public class Dec3NElementParser : ElementParser<float>
{
    public static Dec3NElementParser Singleton = new Dec3NElementParser();

    public override int Count => 3;

    public override (int, int) ParseElement(Span<byte> vertex, int vertexOffsetStart, ReadOnlySpan<char> line, int lineOffsetStart = 0, IFormatProvider? formatProvider = null)
    {
        // We read 3 floats in a temp buffer
        Span<float> tmp = stackalloc float[3];
        (_ , int lineOffset) = base.ParseElement(MemoryMarshal.AsBytes(tmp), 0, line, lineOffsetStart, formatProvider);

        // Pack the floats into a Dec3N
        var dec3n = new Dec3N(tmp[0], tmp[1], tmp[2], 0.0f);

        // Copy the result bytes into the actual vertex buffer
        var dec3nSpan = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref dec3n, 1));
        dec3nSpan.CopyTo(vertex.Slice(vertexOffsetStart, 4));

        // We know to have written only 4 bytes on the vertex
        return (vertexOffsetStart + 4, lineOffset);
    }
}

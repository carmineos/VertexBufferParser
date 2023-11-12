using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VertexBufferParser;

public interface IElementParser
{
    /// <param name="vertex">The chunk representing a single vertex in the vertex buffer</param>
    /// <param name="vertexOffsetStart">The offset into the single vertex where to write to</param>
    /// <param name="line">The line to parse</param>
    /// <param name="lineOffsetStart">The offset of the line where the parse has to start from</param>
    /// <param name="formatProvider">The format provider</param>
    /// <returns>The updated <paramref name="vertexOffsetStart"/> and <paramref name="lineOffsetStart"/> where the parsing stopped at</returns>
    public (int vertexOffsetEnd, int lineOffsetEnd) ParseElement(Span<byte> vertex, int vertexOffsetStart, ReadOnlySpan<char> line, int lineOffsetStart, IFormatProvider? formatProvider = null);
}

public abstract class ElementParser<T> : IElementParser where T : unmanaged, ISpanParsable<T>
{
    public abstract int Count { get; }

    public virtual (int vertexOffsetEnd, int lineOffsetEnd) ParseElement(Span<byte> vertex, int vertexOffsetStart, ReadOnlySpan<char> line, int lineOffsetStart, IFormatProvider? formatProvider = null)
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

public static class ElementParser
{
    public static readonly IElementParser Float = new FloatElementParser();
    public static readonly IElementParser Float2 = new Float2ElementParser();
    public static readonly IElementParser Float3 = new Float3ElementParser();
    public static readonly IElementParser Float4 = new Float4ElementParser();
    public static readonly IElementParser Byte4 = new Byte4ElementParser();
    public static readonly IElementParser Dec3N = new Dec3NElementParser();
    public static readonly IElementParser Half2 = new Half2ElementParser();
    public static readonly IElementParser Half4 = new Half4ElementParser();
}

public class FloatElementParser : ElementParser<float>
{
    public override int Count => 1;
}

public class Float2ElementParser : ElementParser<float>
{
    public override int Count => 2;
}

public class Float3ElementParser : ElementParser<float>
{
    public override int Count => 3;
}

public class Float4ElementParser : ElementParser<float>
{
    public override int Count => 4;
}

public class Byte4ElementParser : ElementParser<byte>
{
    public override int Count => 4;
}

public class Dec3NElementParser : ElementParser<float>
{
    public override int Count => 3;

    public override (int vertexOffsetEnd, int lineOffsetEnd) ParseElement(Span<byte> vertex, int vertexOffsetStart, ReadOnlySpan<char> line, int lineOffsetStart, IFormatProvider? formatProvider = null)
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

public class Half2ElementParser : ElementParser<Half>
{
    public override int Count => 2;
}

public class Half4ElementParser : ElementParser<Half>
{
    public override int Count => 4;
}

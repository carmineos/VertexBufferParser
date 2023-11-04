using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public abstract class VertexComponentParser
{
    public static VertexComponentParser GetVertexComponentParser(SemanticDescriptor semanticDescriptor)
    {
        return semanticDescriptor.Type switch
        {
            "byte" => new VertexComponentParser<byte>(),
            "short" => new VertexComponentParser<short>(),
            "half" => new VertexComponentParser<Half>(),
            "float" => new VertexComponentParser<float>(),
            "int" => new VertexComponentParser<int>(),
            _ => throw new Exception(),
        };
    }

    public abstract (int,int) ParseVertexComponent(Span<byte> vertex, int vertexIndex, int itemsCount, ReadOnlySpan<char> line, int lineStart = 0, IFormatProvider? formatProvider = null);
}

public class VertexComponentParser<T> : VertexComponentParser where T : unmanaged, ISpanParsable<T>
{
    public int ComponentSize => Unsafe.SizeOf<T>();

    public override (int, int) ParseVertexComponent(Span<byte> vertex, int vertexComponentStartIndex, int itemsToRead, ReadOnlySpan<char> line, int lineStartIndex = 0, IFormatProvider? formatProvider = null)
    {
        // Number of items already read
        int itemsRead = 0;

        // Index in the vertex being written
        int vertexIndex = vertexComponentStartIndex;

        // Index in the line being read
        int lineIndex = lineStartIndex;

        formatProvider ??= NumberFormatInfo.InvariantInfo;

        int start = lineIndex;
        int count = 0;

        while (itemsRead < itemsToRead && lineIndex < line.Length)
        {
            var c = line[lineIndex];

            // Found a space, Collect all the previous chars if any
            if (char.IsWhiteSpace(c))
            {
                if (count > 0)
                {
                    var lineChunk = line.Slice(start, count);
                    var vertexChunk = vertex.Slice(vertexIndex, Unsafe.SizeOf<T>());

                    ParseLineChunk(vertexChunk, lineChunk, formatProvider);
                    itemsRead++;
                    vertexIndex += Unsafe.SizeOf<T>();

                    count = 0;
                }

                start = lineIndex + 1;
            }
            else // More chars
            {
                count++;
            }

            lineIndex++;
        }

        // Collect last item
        if (count > 0)
        {
            var lineChunk = line.Slice(start, count);
            var vertexChunk = vertex.Slice(vertexIndex, Unsafe.SizeOf<T>());

            ParseLineChunk(vertexChunk, lineChunk, formatProvider);
            itemsRead++;
            vertexIndex += Unsafe.SizeOf<T>();
        }

        return (vertexIndex, lineIndex);
    }

    public static void ParseLineChunk(Span<byte> buffer, ReadOnlySpan<char> chunk, IFormatProvider? formatProvider = null)
    {
        T parsed = T.Parse(chunk, provider: formatProvider);
        Span<byte> bytes = MemoryMarshal.Cast<T, byte>(MemoryMarshal.CreateSpan(ref parsed, 1));
        bytes.CopyTo(buffer);
    }
}
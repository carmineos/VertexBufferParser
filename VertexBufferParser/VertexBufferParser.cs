using System.Diagnostics;

namespace VertexBufferParser;

public class VertexBufferParser
{
    private readonly IFormatProvider? _formatProvider;

    public ElementDescriptor[] SemanticDescriptors;

    public VertexBufferParser(ElementDescriptor[] semanticDescriptors, IFormatProvider? formatProvider = null)
    {
        SemanticDescriptors = semanticDescriptors;
        _formatProvider = formatProvider;
    }

    public void Parse(Span<byte> vertexBuffer, int vertexStride, ReadOnlySpan<char> verticesString)
    {
        var verticesCount = vertexBuffer.Length / vertexStride;

        var lines = verticesString.EnumerateLines();
        var lineCount = 0;

        foreach (var line in lines)
        {
            // This should only happen in the first and last lines, in case of custom indentation
            if (line.IsWhiteSpace())
                continue;

            // read the vertex on each line
            var vertexChunk = vertexBuffer.Slice(lineCount * vertexStride, vertexStride);

            ParseVertex(vertexChunk, line);
            lineCount++;
        }

        Debug.Assert(verticesCount == lineCount);
    }

    private void ParseVertex(Span<byte> vertex, ReadOnlySpan<char> line)
    {
        var byteIndex = 0;
        var lineIndex = 0;

        // TODO: Pre-compute expression to avoid looping for each vertex
        foreach (var descriptor in SemanticDescriptors)
        {
            var parser = GetVertexElementParser(descriptor);

            (byteIndex, lineIndex) = parser.ParseElement(vertex, byteIndex, line, lineIndex, _formatProvider);
        }
    }

    private static IElementParser GetVertexElementParser(ElementDescriptor semanticDescriptor)
    {
        return semanticDescriptor.Type switch
        {
            "Float" => VertexElementParsers.Float,
            "Float2" => VertexElementParsers.Float2,
            "Float3" => VertexElementParsers.Float3,
            "Float4" => VertexElementParsers.Float4,
            "Dec3N" => VertexElementParsers.Dec3N,
            "Color" => VertexElementParsers.Byte4,
            "Half2" => VertexElementParsers.Half2,
            "Half4" => VertexElementParsers.Half4,
            _ => throw new Exception(),
        };
    }
}

public static class VertexElementParsers
{
    public static readonly IElementParser Float = new ElementParser<float>(1);
    public static readonly IElementParser Float2 = new ElementParser<float>(2);
    public static readonly IElementParser Float3 = new ElementParser<float>(3);
    public static readonly IElementParser Float4 = new ElementParser<float>(4);
    public static readonly IElementParser Byte4 = new ElementParser<byte>(4);
    public static readonly IElementParser Half2 = new ElementParser<Half>(2);
    public static readonly IElementParser Half4 = new ElementParser<Half>(4);
    public static readonly IElementParser UShort = new ElementParser<ushort>(1);
    public static readonly IElementParser Dec3N = new Dec3NElementParser();
}
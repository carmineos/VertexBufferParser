using System.Diagnostics;

namespace VertexBufferParser;

public class VertexBufferParser
{
    private readonly ElementDescriptor[] _elementDescriptors;
    private readonly IFormatProvider? _formatProvider;

    public VertexBufferParser(ElementDescriptor[] semanticDescriptors, IFormatProvider? formatProvider = null)
    {
        _elementDescriptors = semanticDescriptors;
        _formatProvider = formatProvider;
    }

    public void Parse(Span<byte> vertexBuffer, int vertexStride, ReadOnlySpan<char> verticesString)
    {
        var vertexCount = 0;

        var lines = verticesString.EnumerateLines();

        foreach (var line in lines)
        {
            // This should only happen in the first and last lines, in case of custom indentation
            if (line.IsWhiteSpace())
                continue;

            // read the vertex on each line
            var vertexSpan = vertexBuffer.Slice(vertexCount * vertexStride, vertexStride);

            ParseVertex(vertexSpan, line);
            vertexCount++;
        }

        Debug.Assert(vertexBuffer.Length / vertexStride == vertexCount);
    }

    private void ParseVertex(Span<byte> vertexSpan, ReadOnlySpan<char> line)
    {
        var vertexOffset = 0;
        var lineOffset = 0;

        // TODO: Pre-compute expression to avoid looping for each vertex
        foreach (var descriptor in _elementDescriptors)
        {
            var parser = GetVertexElementParser(descriptor);

            (vertexOffset, lineOffset) = parser.ParseElement(vertexSpan, vertexOffset, line, lineOffset, _formatProvider);
        }
    }

    private static IElementParser GetVertexElementParser(ElementDescriptor elementDescriptor)
    {
        return elementDescriptor.Type switch
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
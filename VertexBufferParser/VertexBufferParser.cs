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
            var parser = GetVertexComponentParser(descriptor);

            (byteIndex, lineIndex) = parser.ParseElement(vertex, byteIndex, line, lineIndex, _formatProvider);
        }
    }

    public static IElementParser GetVertexComponentParser(ElementDescriptor semanticDescriptor)
    {
        return semanticDescriptor.Type switch
        {
            "Float" => ElementParser.Float,
            "Float2" => ElementParser.Float2,
            "Float3" => ElementParser.Float3,
            "Float4" => ElementParser.Float4,
            "Dec3N" => ElementParser.Dec3N,
            "Colour" => ElementParser.Byte4,
            "Half2" => ElementParser.Half2,
            "Half4" => ElementParser.Half4,
            _ => throw new Exception(),
        };
    }
}
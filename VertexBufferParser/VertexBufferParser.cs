using System.Diagnostics;

namespace VertexBufferParser;

public class VertexBufferParser
{
    public ElementDescriptor[] SemanticDescriptors;

    public VertexBufferParser(ElementDescriptor[] semanticDescriptors)
    {
        SemanticDescriptors = semanticDescriptors;
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

        foreach (var descriptor in SemanticDescriptors)
        {
            var parser = GetVertexComponentParser(descriptor);

            (byteIndex, lineIndex) = parser.ParseElement(vertex, byteIndex, line, lineIndex, null);
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
using System.Diagnostics;

public class VertexBufferParser
{
    public byte[] Buffer;

    public SemanticDescriptor[] SemanticDescriptors;


    public void ParseVertices(Span<byte> vertexBuffer, int vertexStride, ReadOnlySpan<char> verticesString)
    {
        var verticesCount = vertexBuffer.Length / vertexStride;

        var lines = verticesString.EnumerateLines();
        var lineCount = 0;

        foreach (var line in lines)
        {
            // read the vertex on each line
            var vertexChunk = vertexBuffer.Slice(lineCount * vertexStride, vertexStride);

            ParseVertex(vertexChunk, line);
            lineCount++;
        }

        Debug.Assert(verticesCount == lineCount);
    }

    public void ParseVertex(Span<byte> vertex, ReadOnlySpan<char> line)
    {
        var byteIndex = 0;
        var lineIndex = 0;

        foreach (var descriptor in SemanticDescriptors)
        {
            var parser = VertexComponentParser.GetVertexComponentParser(descriptor);

            (byteIndex, lineIndex) = parser.ParseVertexComponent(vertex, byteIndex, descriptor.Components, line, lineIndex, null);
        }
    }
}
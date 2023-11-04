﻿using System.Diagnostics;

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

            (byteIndex, lineIndex) = parser.ParseElement(vertex, byteIndex, descriptor.Components, line, lineIndex, null);
        }
    }

    public static IElementParser GetVertexComponentParser(ElementDescriptor semanticDescriptor)
    {
        return semanticDescriptor.Type switch
        {
            "byte" => ElementParser<byte>.Singleton,
            "short" => ElementParser<short>.Singleton,
            "half" => ElementParser<Half>.Singleton,
            "float" => ElementParser<float>.Singleton,
            "int" => ElementParser<int>.Singleton,
            _ => throw new Exception(),
        };
    }
}
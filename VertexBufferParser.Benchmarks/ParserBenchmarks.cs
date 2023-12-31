﻿using BenchmarkDotNet.Attributes;

namespace VertexBufferParser.Benchmarks;

[MemoryDiagnoser]
public class ParserBenchmarks
{
    private const int vertexStride = 40;

    [Params(1_000, 10_000, 100_000, 1_000_000)]
    public int verticesCount { get; set; }

    private VertexBuffer VertexBuffer;
    private VertexBufferParser VertexBufferParser;

    private string verticesString;

    [GlobalSetup]
    public void Setup()
    {
        VertexBuffer = new VertexBuffer()
        {
            VertexLayout = new VertexLayout()
            {
                ElementDescriptors =
                [
                    new ElementDescriptor() { Name = "Position", Type = "Float3" },
                    new ElementDescriptor() { Name = "Normals", Type = "Dec3N" },
                    new ElementDescriptor() { Name = "Color0", Type = "Color" },
                    new ElementDescriptor() { Name = "Color1", Type = "Color" },
                    new ElementDescriptor() { Name = "Texcoords0", Type = "Float2" },
                    new ElementDescriptor() { Name = "Texcoords1", Type = "Float2" },
                ]
            }
        };

        VertexBuffer.Vertices = new byte[vertexStride * verticesCount];

        verticesString = string.Join(Environment.NewLine, Enumerable
            .Range(0, verticesCount)
            .Select(c => "0.1 0.2 0.3    1 1 1    255 255 255 255    128 128 128 128    0.5 0.5    0.5 0.5"));

        VertexBufferParser = new VertexBufferParser(VertexBuffer.VertexLayout.ElementDescriptors);
    }

    [Benchmark]
    public void Parse()
    {
        VertexBufferParser.Parse(VertexBuffer.Vertices, verticesString);
    }
}
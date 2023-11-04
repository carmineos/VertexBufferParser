using BenchmarkDotNet.Attributes;

namespace VertexBufferParser.Benchmarks;

[MemoryDiagnoser]
public class ParserBenchmarks
{
    private const int vertexStride = 28;
    private const int verticesCount = 1_000_000;

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
                SemanticDescriptors =
                [
                    new SemanticDescriptor() { Name = "Position", Components = 3, Type = "float" },
                    new SemanticDescriptor() { Name = "Normals", Components = 3, Type = "float" },
                    new SemanticDescriptor() { Name = "Color", Components = 4, Type = "byte" },
                ]
            }
        };

        verticesString = string.Join(Environment.NewLine, Enumerable
            .Range(0, verticesCount)
            .Select(c => "      0.1 0.2     0.3 1 1 1        255 255    255 255     ")); // bad formatted lines (spaces/tabs) aren't a problem

        VertexBufferParser = new VertexBufferParser();
        VertexBufferParser.SemanticDescriptors = VertexBuffer.VertexLayout.SemanticDescriptors.ToArray();
        VertexBufferParser.Buffer = new byte[vertexStride * verticesCount];
    }

    [Benchmark]
    public void Parse()
    {
        VertexBufferParser.ParseVertices(VertexBufferParser.Buffer, vertexStride, verticesString);
    }
}
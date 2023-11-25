using BenchmarkDotNet.Attributes;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace VertexBufferParser.Benchmarks;

[MemoryDiagnoser]
public class WriterBenchmarks
{
    private const int vertexStride = 48;

    [Params(1_000, 10_000, 100_000, 1_000_000)]
    public int verticesCount { get; set; }

    private VertexBuffer VertexBuffer;
    private VertexBufferWriter VertexBufferWriter;

    private TextWriter writer;

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
                    new ElementDescriptor() { Name = "Normals", Type = "Float3" },
                    new ElementDescriptor() { Name = "Color0", Type = "Color" },
                    new ElementDescriptor() { Name = "Texcoords0", Type = "Float2" },
                    new ElementDescriptor() { Name = "Tangents", Type = "Float3" },
                ]
            }
        };

        VertexBuffer.Vertices = new byte[vertexStride * verticesCount];

        var vertex = new Vertex()
        {
            Position = new Vector3(0.1f, 0.2f, 0.3f),
            Normals = new Vector3(1.0f, 1.0f, 1.0f),
            Color0 = new Color(255, 255, 255, 255),
            Texcoords0 = new Vector2(0.5f, 0.5f),
            Tangents = new Vector3(1.0f, 0.0f, 0.0f)
        };

        var verticesSpan = MemoryMarshal.Cast<byte, Vertex>(VertexBuffer.Vertices);
        verticesSpan.Fill(vertex);


        VertexBufferWriter = new VertexBufferWriter(VertexBuffer.VertexLayout.ElementDescriptors);

        var sb = new StringBuilder();
        writer = new StringWriter(sb);
    }

    [Benchmark]
    public void Write()
    {
        VertexBufferWriter.Write(VertexBuffer.Vertices, vertexStride, writer);
    }
}
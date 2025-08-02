using BenchmarkDotNet.Attributes;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace VertexBufferParser.Benchmarks;

[MemoryDiagnoser]
public class PositionOffsetBenchmarks
{
    private const int vertexStride = 48;

    [Params(1_000, 10_000, 100_000, 1_000_000)]
    public int verticesCount { get; set; }

    private VertexBuffer VertexBuffer;

    [GlobalSetup]
    public void Setup()
    {
        VertexBuffer = new VertexBuffer()
        {
            VertexLayout = new VertexLayout()
            {
                ElementDescriptors =
                [
                    new ElementDescriptor() { Name = ElementDescriptorName.Position, Type = "Float3" },
                    new ElementDescriptor() { Name = ElementDescriptorName.Normals, Type = "Float3" },
                    new ElementDescriptor() { Name = ElementDescriptorName.Color0, Type = "Color" },
                    new ElementDescriptor() { Name = ElementDescriptorName.Texcoords0, Type = "Float2" },
                    new ElementDescriptor() { Name = ElementDescriptorName.Tangents, Type = "Float3" },
                ]
            }
        };
        
        VertexBuffer.VertexLayout.ComputeOffsetsAndSizes();

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
    }

    [Benchmark]
    public void PositionOffset()
    {
        var offset = new Vector3(1.0f, 2.0f, 3.0f);

        var positions = VertexBuffer.GetElementSpan<Vector3>(ElementDescriptorName.Position);
    
        for (int i = 0; i < positions.Length; i++)
        {
            ref var position = ref positions[i];
            position += offset;
        }
    }
}
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Serialization;
using VertexBufferParser;

var xml =
    $"""
    <Geometry>
      <VertexBuffer>
        <VertexLayout>
          <Elements>
            <Element name="Position" type="Float3" />
            <Element name="Normals" type="Float3" />
            <Element name="Color0" type="Color" />
            <Element name="Texcoords0" type="Float2" />
            <Element name="Tangents" type="Float3" />
          </Elements>
        </VertexLayout>
        <VerticesCount>24</VerticesCount>
        <Vertices>
          -1.5 -1 -0.5    0 0 -1    255 255 255 255    0 0    1 0 0
          1.5 -1 -0.5    0 0 -1    255 255 255 255    1 0    1 0 0
          -1.5 1 -0.5   0 0 -1    255 255 255 255    0 1    1 0 0
          1.5 1 -0.5    0 0 -1    255 255 255 255    1 1    1 0 0
          -1.5 -1 0.5    0 0 1    255 255 255 255    1 0    1 0 0
          1.5 -1 0.5    0 0 1    255 255 255 255    0 0    1 0 0
          -1.5 1 0.5    0 0 1    255 255 255 255    1 1    1 0 0
          1.5 1 0.5    0 0 1    255 255 255 255    0 1    1 0 0
          -1.5 -1 -0.5    0 -1 0    255 255 255 255    0 1    1 0 0
          1.5 -1 -0.5    0 -1 0    255 255 255 255    1 1    1 0 0
          -1.5 -1 0.5    0 -1 0    255 255 255 255    0 0    1 0 0
          1.5 -1 0.5    0 -1 0    255 255 255 255    1 0    1 0 0
          -1.5 1 -0.5    0 1 0    255 255 255 255    1 1    1 0 0
          1.5 1 -0.5    0 1 0    255 255 255 255    0 1    1 0 0
          -1.5 1 0.5    0 1 0    255 255 255 255    1 0    1 0 0
          1.5 1 0.5    0 1 0    255 255 255 255    0 0    1 0 0
          -1.5 -1 -0.5    -1 0 0    255 255 255 255    0 0    0 1 0
          -1.5 1 -0.5    -1 0 0    255 255 255 255    1 0    0 1 0
          -1.5 -1 0.5    -1 0 0    255 255 255 255    0 1    0 1 0
          -1.5 1 0.5    -1 0 0    255 255 255 255    1 1    0 1 0
          1.5 -1 -0.5    1 0 0    255 255 255 255    0 1    0 1 0
          1.5 1 -0.5    1 0 0    255 255 255 255    1 1    0 1 0
          1.5 -1 0.5    1 0 0    255 255 255 255    0 0    0 1 0
          1.5 1 0.5    1 0 0    255 255 255 255    1 0    0 1 0
        </Vertices>
      </VertexBuffer>
      <IndexBuffer>
        <Element name="Index" type="UShort" />
        <IndicesCount>36</IndicesCount>
        <Indices>
          0 2 1 1 2 3 4 5 6 5
          7 6 8 9 10 9 11 10 12 14
          13 13 14 15 16 18 17 17 18 19
          20 21 22 21 23 22
        </Indices>
      </IndexBuffer>
    </Geometry>
    """;

var serializer = new XmlSerializer(typeof(Geometry));
using var stream = new StringReader(xml);
Geometry geometry = (Geometry)serializer.Deserialize(stream)!;

geometry.VertexBuffer.VertexLayout.ComputeOffsetsAndSizes();

VertexBuffer vertexBuffer = geometry.VertexBuffer;
IndexBuffer indexBuffer = geometry.IndexBuffer;

ParseVertices();
ParseIndices();
//Dump();

UpdatePosition(vertexBuffer, new Vector3(10, 0, 0));
UpdateColor0(vertexBuffer, new Color(33, 66, 99, 127));
Dump();

WriteVertices();
WriteIndices();


void WriteVertices()
{
    var sb = new StringBuilder();
    using var sw = new StringWriter(sb);

    var vertexStride = Unsafe.SizeOf<Vertex>();

    new VertexBufferWriter(vertexBuffer.VertexLayout.ElementDescriptors)
        .Write(vertexBuffer.Vertices, sw);

    sw.Flush();
    File.WriteAllText("vertices.txt", sb.ToString());
}

void WriteIndices()
{
    var sb = new StringBuilder();
    using var sw = new StringWriter(sb);

    var vertexStride = Unsafe.SizeOf<Vertex>();

    new IndexBufferWriter(indexBuffer.ElementDescriptor)
        .Write(indexBuffer.Indices, sw);

    sw.Flush();
    File.WriteAllText("indices.txt", sb.ToString());
}

void ParseVertices()
{
    var vertexStride = Unsafe.SizeOf<Vertex>();
    
    vertexBuffer.Vertices = new byte[vertexStride * vertexBuffer.VerticesCount];
    
    new VertexBufferParser.VertexBufferParser(vertexBuffer.VertexLayout.ElementDescriptors)
        .Parse(vertexBuffer.Vertices, vertexBuffer.VerticesText);
}

void ParseIndices()
{
    indexBuffer.Indices = new byte[2 * indexBuffer.IndicesCount];

    new IndexBufferParser(indexBuffer.ElementDescriptor)
        .Parse(indexBuffer.Indices, indexBuffer.IndicesText);
}

void Dump()
{
    var vertices = vertexBuffer.GetTypedVertices<Vertex>();

    for (int i = 0; i < vertices.Length; i++)
    {
        Console.WriteLine($"{i}: {vertices[i]}");
    }

    var indices = MemoryMarshal.Cast<byte, ushort>(indexBuffer.Indices);

    for (int i = 0; i < indices.Length; i++)
    {
        Console.WriteLine($"{i}: {indices[i]}");
    }
}

void UpdatePosition(VertexBuffer buffer, Vector3 offset)
{
    var positions = buffer.GetElementSpan<Vector3>(ElementDescriptorName.Position);
    
    for (int i = 0; i < positions.Length; i++)
    {
        ref var position = ref positions[i];
        position += offset;
    }
}

void UpdatePositionEnumerator(VertexBuffer buffer, Vector3 offset)
{
    var positions = buffer.GetElementSpan<Vector3>(ElementDescriptorName.Position);
    
    foreach (ref var position in positions)
    {
        position += offset;
    }
}

void UpdateColor0(VertexBuffer buffer, Color color)
{
    var colors = buffer.GetElementSpan<Color>(ElementDescriptorName.Color0);
    
    for (int i = 0; i < colors.Length; i++)
    {
        colors[i] = color;
    }
}


public readonly record struct Vertex(
    Vector3 Position,
    Vector3 Normals,
    Color Color0,
    Vector2 Texcoords0,
    Vector3 Tangents);

public readonly record struct Color(byte R, byte G, byte B, byte A);
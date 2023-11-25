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
        <Element semantic="Index" type="UShort" />
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

VertexBuffer vertexBuffer = geometry.VertexBuffer;
IndexBuffer indexBuffer = geometry.IndexBuffer;

ParseVertices();
ParseIndices();
//Dump();

WriteVertices();


void WriteVertices()
{
    var sb = new StringBuilder();
    using var sw = new StringWriter(sb);

    var vertexStride = Unsafe.SizeOf<Vertex>();

    new VertexBufferWriter(vertexBuffer.VertexLayout.ElementDescriptors)
        .Write(vertexBuffer.Vertices, vertexStride, sw);

    sw.Flush();
    File.WriteAllText("output.txt", sb.ToString());
}

void ParseVertices()
{
    var vertexStride = Unsafe.SizeOf<Vertex>();
    
    vertexBuffer.Vertices = new byte[vertexStride * vertexBuffer.VerticesCount];
    
    new VertexBufferParser.VertexBufferParser(vertexBuffer.VertexLayout.ElementDescriptors)
        .Parse(vertexBuffer.Vertices, vertexStride, vertexBuffer.VerticesText);
}

void ParseIndices()
{
    indexBuffer.Indices = new byte[2 * indexBuffer.IndicesCount];

    new IndexBufferParser(indexBuffer.ElementDescriptor)
        .Parse(indexBuffer.Indices, indexBuffer.IndicesText);
}

void Dump()
{
    var vertices = MemoryMarshal.Cast<byte, Vertex>(vertexBuffer.Vertices);

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

public readonly record struct Vertex(
    Vector3 Position,
    Vector3 Normals,
    Color Color0,
    Vector2 Texcoords0,
    Vector3 Tangents);

public readonly record struct Color(byte R, byte G, byte B, byte A);
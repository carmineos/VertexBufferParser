using System.Numerics;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using VertexBufferParser;

var xml =
    """
    <VertexBuffer>
      <VertexLayout>
        <Elements>   
          <Element semantic="Position" type="float" components="3" />
          <Element semantic="Normals" type="float" components="3" />
          <Element semantic="Color0" type="byte" components="4" />
          <Element semantic="Color1" type="byte" components="4" />
          <Element semantic="Texcoords0" type="float" components="2" />
          <Element semantic="Texcoords1" type="float" components="2" />
        </Elements>
      </VertexLayout>
     <Vertices>
        0.1 0.2 0.3    255 255 255 255    128 128 128 128    0.5 0.5    0.5 0.5
        0.4 0.5 0.6    255 255 255 255    128 128 128 128    0.5 0.5    0.5 0.5
        ...
        ...
     </Vertices>
    </VertexBuffer>
    """;

var serializer = new XmlSerializer(typeof(VertexBuffer));
using var stream = new StringReader(xml);
VertexBuffer vertexBuffer = (VertexBuffer)serializer.Deserialize(stream)!;

int vertexStride = 48;
int verticesCount = 1_000_000;
vertexBuffer.Vertices = new byte[vertexStride * verticesCount];

var verticesString = string.Join(Environment.NewLine, Enumerable
    .Range(0, verticesCount)
    .Select(c => "      0.1 0.2     0.3 1 1 1        255 255    255 255         128 128 128 128    0.5 0.5    0.5 0.5")); // bad formatted lines (spaces/tabs) aren't a problem

var vertexBufferParser = new VertexBufferParser.VertexBufferParser(vertexBuffer.VertexLayout.ElementDescriptors);
vertexBufferParser.Parse(vertexBuffer.Vertices, vertexStride, verticesString);

// Print expected values
//Dump();

void Dump()
{
    var dump = MemoryMarshal.Cast<byte, Vertex>(vertexBuffer.Vertices);
    for (int i = 0; i < dump.Length; i++)
    {
        Console.WriteLine($"{i}: {dump[i]}");
    }
}

public readonly record struct Vertex(
    Vector3 Position,
    Vector3 Normals,
    Color Color0,
    Color Color1,
    Vector2 Texcoords0,
    Vector2 Texcoords1);

public readonly record struct Color(byte R, byte G, byte B, byte A);
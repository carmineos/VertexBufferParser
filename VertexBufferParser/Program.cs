using System;
using System.Numerics;
using System.Runtime.InteropServices;
using VertexBufferParser;

var xml =
    """
    <VertexBuffer>
      <VertexLayout>
        <Semantics>   
          <Semantic name="Position" type="float" components="3" />
          <Semantic name="Color" type="byte" components="4" />
        </Semantics>
      </VertexLayout>
     <Vertices>
        0.1 0.2 0.3 255 255 255 255
        0.4 0.5 0.6 255 255 255 255
     </Vertices>
    <VertexBuffer>
    """;

int vertexStride = 28;
int verticesCount = 1_000_000;

var verticesString = string.Join(Environment.NewLine, Enumerable
    .Range(0, verticesCount)
    .Select(c => "      0.1 0.2     0.3 1 1 1        255 255    255 255     ")); // bad formatted lines (spaces/tabs) aren't a problem

var vertexBuffer = new VertexBuffer()
{
    VertexLayout = new VertexLayout()
    {
        SemanticDescriptors =
        [
            new SemanticDescriptor() { Name = "Position", Components = 3, Type = "float" },
            new SemanticDescriptor() { Name = "Normals", Components = 3, Type = "float" },
            new SemanticDescriptor() { Name = "Color", Components = 4, Type = "byte" },
        ]
    },
    Vertices = new byte[vertexStride * verticesCount],
};

var vertexBufferParser = new VertexBufferParser.VertexBufferParser(vertexBuffer.VertexLayout.SemanticDescriptors);
vertexBufferParser.Parse(vertexBuffer.Vertices, vertexStride, verticesString);

// Print expected values
//var test = MemoryMarshal.Cast<byte, Vertex>(vertexBuffer.Vertices);
//for (int i = 0; i < test.Length; i++)
//{
//    Console.WriteLine($"{i}: {test[i]}");
//}

public struct Vertex
{
    public Vector3 Position { get; set; }

    public Vector3 Normals { get; set; }

    public uint Color { get; set; }

    public override string ToString()
    {
        return Position.ToString() + "; " + Normals.ToString() + "; " + Color.ToString();
    }
}
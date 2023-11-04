using System.Xml.Serialization;

namespace VertexBufferParser;

public class VertexBuffer
{
    public VertexLayout VertexLayout { get; set; }
    public byte[] Vertices { get; set; }
}

public class VertexLayout
{
    public SemanticDescriptor[] SemanticDescriptors { get; set; }
}

public struct SemanticDescriptor
{
    public string Name { get; set; }
    public string Type { get; set; }
    public int Components { get; set; }
}
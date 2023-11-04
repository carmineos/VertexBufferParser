using System.Xml.Serialization;

public class VertexBuffer
{
    public VertexLayout VertexLayout { get; set; }
    public byte[] Vertices { get; set; }
}

public class VertexLayout
{
    [XmlArray]
    [XmlArrayItem(ElementName = "Semantic", Type = typeof(SemanticDescriptor))]
    public List<SemanticDescriptor> SemanticDescriptors { get; set; }
}

public class SemanticDescriptor
{
    public string Name { get; set; }
    public string Type { get; set; }
    public int Components { get; set; }
}
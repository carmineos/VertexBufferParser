using System.Xml.Serialization;

namespace VertexBufferParser;

[Serializable]
public class VertexBuffer
{
    public VertexLayout VertexLayout { get; set; }

    [XmlIgnore]
    public byte[] Vertices { get; set; }
}

[Serializable]
public class VertexLayout
{
    [XmlArray("Elements")]
    [XmlArrayItem("Element")]
    public ElementDescriptor[] ElementDescriptors { get; set; }
}

[Serializable]
public struct ElementDescriptor
{
    [XmlAttribute(AttributeName = "semantic")]
    public string Semantic { get; set; }

    [XmlAttribute(AttributeName = "type")]
    public string Type { get; set; }

    [XmlAttribute(AttributeName = "components")]
    public int Components { get; set; }
}
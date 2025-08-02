using System.Xml.Serialization;

namespace VertexBufferParser;

[Serializable]
public class Geometry
{
    public VertexBuffer VertexBuffer { get; set; }
    public IndexBuffer IndexBuffer { get; set; }
}


[Serializable]
public class VertexBuffer
{
    public VertexLayout VertexLayout { get; set; }

    public int VerticesCount { get; set; }

    [XmlIgnore]
    public byte[] Vertices { get; set; }

    [XmlElement(ElementName = "Vertices")]
    public string VerticesText { get; set; }
}

[Serializable]
public class VertexLayout
{
    [XmlArray("Elements")]
    [XmlArrayItem("Element")]
    public ElementDescriptor[] ElementDescriptors { get; set; }
    
    public void ComputeOffsetsAndSizes()
    {
        int offset = 0;
        
        foreach (var element in ElementDescriptors)
        {
            element.Offset = offset;
            element.Size = element.GetElementSize();
            offset += element.Size;
        }
    }
}

[Serializable]
public class ElementDescriptor
{
    [XmlAttribute(AttributeName = "name")]
    public string Name { get; set; }

    [XmlAttribute(AttributeName = "type")]
    public string Type { get; set; }
    
    [XmlIgnore]
    public int Offset { get; set; }
    
    [XmlIgnore]
    public int Size { get; set; }
}

[Serializable]
public class IndexBuffer
{
    [XmlElement("Element")]
    public ElementDescriptor ElementDescriptor { get; set; }
    
    public int IndicesCount { get; set; }

    [XmlIgnore]
    public byte[] Indices { get; set; }

    [XmlElement(ElementName = "Indices")]
    public string IndicesText { get; set; }
}
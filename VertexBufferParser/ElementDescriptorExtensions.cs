namespace VertexBufferParser;

public static class ElementDescriptorExtensions
{
    public static int GetElementSize(this ElementDescriptor elementDescriptor)
    {
        return elementDescriptor.Type switch
        {
            ElementDescriptorType.Float => 4,
            ElementDescriptorType.Float2 => 8,
            ElementDescriptorType.Float3 => 12,
            ElementDescriptorType.Float4 => 16,
            ElementDescriptorType.Dec3N => 4,
            ElementDescriptorType.Color => 4,
            ElementDescriptorType.Half2 => 4,
            ElementDescriptorType.Half4 => 8,
            ElementDescriptorType.UShort => 2,
            ElementDescriptorType.UInt => 4,
            _ => throw new Exception(),
        };
    }

    public static int ComputeVertexStride(ElementDescriptor[] elementDescriptors)
    {
        int stride = 0;

        for (int i = 0; i < elementDescriptors.Length; i++)
        {
            stride += elementDescriptors[i].GetElementSize();
        }

        return stride;
    }
    
    public static ElementDescriptor GetDescriptor(ElementDescriptor[] elementDescriptors, ElementDescriptorName name)
    {
        foreach (var desc in elementDescriptors)
        {
            if (desc.Name == name)
                return desc;
        }
        
        throw new ArgumentException($"Descriptor with name '{name}' not found.", nameof(name));
    }
}
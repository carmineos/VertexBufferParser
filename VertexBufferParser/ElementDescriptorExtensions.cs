namespace VertexBufferParser;

public static class ElementDescriptorExtensions
{
    public static int GetElementSize(this ElementDescriptor elementDescriptor)
    {
        return elementDescriptor.Type switch
        {
            "Float" => 4,
            "Float2" => 8,
            "Float3" => 12,
            "Float4" => 16,
            "Dec3N" => 4,
            "Color" => 4,
            "Half2" => 4,
            "Half4" => 8,
            "UShort" => 2,
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
}
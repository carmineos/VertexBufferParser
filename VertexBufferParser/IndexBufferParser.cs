namespace VertexBufferParser;

public class IndexBufferParser
{
    private readonly ElementDescriptor _elementDescriptor;
    private readonly IFormatProvider? _formatProvider;

    public IndexBufferParser(ElementDescriptor elementDescriptor, IFormatProvider? formatProvider = null)
    {
        _elementDescriptor = elementDescriptor;
        _formatProvider = formatProvider;
    }

    public void Parse(Span<byte> indexBuffer, ReadOnlySpan<char> indicesString)
    {
        var parser = GetIndexParser(_elementDescriptor);
        _ = parser.ParseElement(indexBuffer, 0, indicesString, 0, _formatProvider);
    }

    public static IElementParser GetIndexParser(ElementDescriptor elementDescriptor)
    {
        return elementDescriptor.Type switch
        {
            ElementDescriptorType.UShort => new ElementParser<ushort>(),
            _ => throw new Exception(),
        };
    }
}
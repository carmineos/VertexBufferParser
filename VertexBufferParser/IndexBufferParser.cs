using System.Diagnostics;

namespace VertexBufferParser;

public class IndexBufferParser
{
    private readonly IFormatProvider? _formatProvider;

    public ElementDescriptor ElementDescriptor;

    public IndexBufferParser(ElementDescriptor elementDescriptor, IFormatProvider? formatProvider = null)
    {
        ElementDescriptor = elementDescriptor;
        _formatProvider = formatProvider;
    }

    public void Parse(Span<byte> indexBuffer, ReadOnlySpan<char> indicesString)
    {
        var parser = GetIndexComponentParser(ElementDescriptor);
        (_, _) = parser.ParseElement(indexBuffer, 0, indicesString, 0, _formatProvider);
    }

    public static IElementParser GetIndexComponentParser(ElementDescriptor semanticDescriptor)
    {
        return semanticDescriptor.Type switch
        {
            "UShort" => new ElementParser<ushort>(),
            _ => throw new Exception(),
        };
    }
}
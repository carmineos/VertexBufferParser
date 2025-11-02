namespace VertexBufferParser;

public class VertexBufferParser
{
    private readonly ElementDescriptor[] _elementDescriptors;
    private readonly IFormatProvider? _formatProvider;
    
    public const string DefaultVerticesSeparator = "\r\n";
    public const string DefaultElementsSeparator = "    ";

    public string VerticesSeparator { get; set; } = DefaultVerticesSeparator;
    public string ElementsSeparator { get; set; } = DefaultElementsSeparator;

    public VertexBufferParser(ElementDescriptor[] semanticDescriptors, IFormatProvider? formatProvider = null)
    {
        _elementDescriptors = semanticDescriptors;
        _formatProvider = formatProvider;
    }

    public void Parse(Span<byte> vertexBuffer, ReadOnlySpan<char> verticesString)
    {
        var vertexCount = 0;
        var vertexStride = ElementDescriptorExtensions.ComputeVertexStride(_elementDescriptors);
        
        var verticesSpans = new VerticesEnumerator(verticesString.Trim(), VerticesSeparator);

        foreach (var line in verticesSpans)
        {
            // read the vertex on each line
            var vertexSpan = vertexBuffer.Slice(vertexCount * vertexStride, vertexStride);

            ParseVertex(vertexSpan, line);
            vertexCount++;
        }
    }

    private void ParseVertex(Span<byte> vertexSpan, ReadOnlySpan<char> line)
    {
        var vertexOffset = 0;
        var lineOffset = 0;

        // TODO: Pre-compute expression to avoid looping for each vertex
        foreach (var descriptor in _elementDescriptors)
        {
            var parser = GetVertexElementParser(descriptor);

            (vertexOffset, lineOffset) = parser.ParseElement(vertexSpan, vertexOffset, line, lineOffset, _formatProvider);
        }
    }

    private static IElementParser GetVertexElementParser(ElementDescriptor elementDescriptor)
    {
        return elementDescriptor.Type switch
        {
            ElementDescriptorType.Float => VertexElementParsers.Float,
            ElementDescriptorType.Float2 => VertexElementParsers.Float2,
            ElementDescriptorType.Float3 => VertexElementParsers.Float3,
            ElementDescriptorType.Float4 => VertexElementParsers.Float4,
            ElementDescriptorType.Dec3N => VertexElementParsers.Dec3N,
            ElementDescriptorType.Color => VertexElementParsers.Byte4,
            ElementDescriptorType.Half2 => VertexElementParsers.Half2,
            ElementDescriptorType.Half4 => VertexElementParsers.Half4,
            _ => throw new Exception(),
        };
    }
}

public static class VertexElementParsers
{
    public static readonly IElementParser Float = new ElementParser<float>(1);
    public static readonly IElementParser Float2 = new ElementParser<float>(2);
    public static readonly IElementParser Float3 = new ElementParser<float>(3);
    public static readonly IElementParser Float4 = new ElementParser<float>(4);
    public static readonly IElementParser Byte4 = new ElementParser<byte>(4);
    public static readonly IElementParser Half2 = new ElementParser<Half>(2);
    public static readonly IElementParser Half4 = new ElementParser<Half>(4);
    public static readonly IElementParser UShort = new ElementParser<ushort>(1);
    public static readonly IElementParser Dec3N = new Dec3NElementParser();
}
namespace VertexBufferParser;

public class VertexBufferWriter
{
    public const string DefaultVerticesSeparator = "\r\n";
    public const string DefaultElementsSeparator = "    ";

    private readonly ElementDescriptor[] _elementDescriptors;
    private readonly IFormatProvider? _formatProvider;

    public string VerticesSeparator { get; set; } = DefaultVerticesSeparator;
    public string ElementsSeparator { get; set; } = DefaultElementsSeparator;

    public VertexBufferWriter(ElementDescriptor[] semanticDescriptors, IFormatProvider? formatProvider = null)
    {
        _elementDescriptors = semanticDescriptors;
        _formatProvider = formatProvider;
    }

    public void Write(Span<byte> vertexBuffer, int vertexStride, TextWriter writer)
    {
        var vertexCount = vertexBuffer.Length / vertexStride;

        // TODO: Is 512 enough to cover all the cases? Otherwise consider using ArrayPool
        Span<char> lineBuffer = stackalloc char[512];

        for (int i = 0; i < vertexCount; i++)
        {
            var vertexSpan = vertexBuffer.Slice(i * vertexStride, vertexStride);
            int charsWritten = WriteVertex(vertexSpan, lineBuffer);

            if (i < vertexCount - 1)
            {
                // Copy the separator and increase the length
                VerticesSeparator.CopyTo(lineBuffer.Slice(charsWritten, VerticesSeparator.Length));
                charsWritten += VerticesSeparator.Length;
            }

            var chars = lineBuffer.Slice(0, charsWritten);
            writer.Write(chars);
        }
    }

    private int WriteVertex(Span<byte> vertexSpan, Span<char> destination)
    {
        var vertexOffset = 0;
        var charsWritten = 0;

        for (int i = 0; i < _elementDescriptors.Length; i++)
        {
            var descriptor = _elementDescriptors[i];

            var elementSize = descriptor.GetElementSize();
            var elementSpan = vertexSpan.Slice(vertexOffset, elementSize);

            var elementWriter = GetVertexElementWriter(descriptor);

            charsWritten += elementWriter.WriteElement(elementSpan, destination.Slice(charsWritten), _formatProvider);

            vertexOffset += elementSize;

            if (i <  _elementDescriptors.Length - 1)
            {
                // Copy the separator and increase the length
                ElementsSeparator.CopyTo(destination.Slice(charsWritten, ElementsSeparator.Length));
                charsWritten += ElementsSeparator.Length;
            }
        }

        return charsWritten;
    }

    private static IElementWriter GetVertexElementWriter(ElementDescriptor elementDescriptor)
    {
        return elementDescriptor.Type switch
        {
            "Float" => VertexElementWriters.Float,
            "Float2" => VertexElementWriters.Float2,
            "Float3" => VertexElementWriters.Float3,
            "Float4" => VertexElementWriters.Float4,
            "Dec3N" => VertexElementWriters.Dec3N,
            "Color" => VertexElementWriters.Byte4,
            "Half2" => VertexElementWriters.Half2,
            "Half4" => VertexElementWriters.Half4,
            _ => throw new Exception(),
        };
    }
}

public static class VertexElementWriters
{
    private const string floatFormat = "G9";

    public static readonly IElementWriter Float = new ElementWriter<float>(1, format: floatFormat);
    public static readonly IElementWriter Float2 = new ElementWriter<float>(2, format: floatFormat);
    public static readonly IElementWriter Float3 = new ElementWriter<float>(3, format: floatFormat);
    public static readonly IElementWriter Float4 = new ElementWriter<float>(4, format: floatFormat);
    public static readonly IElementWriter Byte4 = new ElementWriter<byte>(4);
    public static readonly IElementWriter Half2 = new ElementWriter<Half>(2, format: floatFormat);
    public static readonly IElementWriter Half4 = new ElementWriter<Half>(4, format: floatFormat);
    public static readonly IElementWriter UShort = new ElementWriter<ushort>(1);
    public static readonly IElementWriter Dec3N = new Dec3NElementWriter(format: floatFormat);
}
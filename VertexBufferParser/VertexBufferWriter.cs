namespace VertexBufferParser;

public class VertexBufferWriter
{
    private const string _verticesSeparator = "\r\n";
    private const string _elementsSeparator = "    ";

    private readonly ElementDescriptor[] _elementDescriptors;
    private readonly IFormatProvider? _formatProvider;

    public VertexBufferWriter(ElementDescriptor[] semanticDescriptors, IFormatProvider? formatProvider = null)
    {
        _elementDescriptors = semanticDescriptors;
        _formatProvider = formatProvider;
    }

    // TODO: Replace writer with Span destination
    public void Write(Span<byte> vertexBuffer, int vertexStride, TextWriter writer, string verticesSeparator = _verticesSeparator)
    {
        var vertexCount = vertexBuffer.Length / vertexStride;

        // TODO: Consider ArrayPool
        Span<char> lineBuffer = stackalloc char[1024];
        lineBuffer.Fill(' ');

        for (int i = 0; i < vertexCount; i++)
        {
            var vertexSpan = vertexBuffer.Slice(i * vertexStride, vertexStride);
            int charsWritten = WriteVertex(vertexSpan, lineBuffer);

            if (i < vertexCount - 1)
            {
                // Copy the separator and increase the length
                verticesSeparator.CopyTo(lineBuffer.Slice(charsWritten, verticesSeparator.Length));
                charsWritten += verticesSeparator.Length;
            }

            var chars = lineBuffer.Slice(0, charsWritten);
            writer.Write(chars);
        }
    }

    private int WriteVertex(Span<byte> vertexSpan, Span<char> destination, string elementsSeparator = _elementsSeparator)
    {
        var vertexOffset = 0;
        var charsWritten = 0;

        for (int i = 0; i < _elementDescriptors.Length; i++)
        {
            var descriptor = _elementDescriptors[i];

            var elementSize = GetVertexElementSize(descriptor);
            var elementSpan = vertexSpan.Slice(vertexOffset, elementSize);

            var elementWriter = GetVertexElementWriter(descriptor);

            charsWritten += elementWriter.WriteElement(elementSpan, destination.Slice(charsWritten), _formatProvider);

            vertexOffset += elementSize;

            if (i <  _elementDescriptors.Length - 1)
            {
                // Copy the separator and increase the length
                elementsSeparator.CopyTo(destination.Slice(charsWritten, elementsSeparator.Length));
                charsWritten += elementsSeparator.Length;
            }
        }

        return charsWritten;
    }

    private IElementWriter GetVertexElementWriter(ElementDescriptor elementDescriptor)
    {
        return elementDescriptor.Type switch
        {
            "Float" => VertexElementWriters.Float,
            "Float2" => VertexElementWriters.Float2,
            "Float3" => VertexElementWriters.Float3,
            "Float4" => VertexElementWriters.Float4,
            //"Dec3N" => VertexElementWriters.Dec3N,
            "Color" => VertexElementWriters.Byte4,
            "Half2" => VertexElementWriters.Half2,
            "Half4" => VertexElementWriters.Half4,
            _ => throw new Exception(),
        };
    }

    private static int GetVertexElementSize(ElementDescriptor elementDescriptor)
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
            _ => throw new Exception(),
        };
    }
}

public static class VertexElementWriters
{
    private const string floatFormat = "0.0";
    //private const string floatFormat = "0.000000";

    public static readonly IElementWriter Float = new ElementWriter<float>(1, format: floatFormat);
    public static readonly IElementWriter Float2 = new ElementWriter<float>(2, format: floatFormat);
    public static readonly IElementWriter Float3 = new ElementWriter<float>(3, format: floatFormat);
    public static readonly IElementWriter Float4 = new ElementWriter<float>(4, format: floatFormat);
    public static readonly IElementWriter Byte4 = new ElementWriter<byte>(4);
    public static readonly IElementWriter Half2 = new ElementWriter<Half>(2, format: floatFormat);
    public static readonly IElementWriter Half4 = new ElementWriter<Half>(4, format: floatFormat);
    public static readonly IElementWriter UShort = new ElementWriter<ushort>(1);
    //public static readonly IElementWriter Dec3N = new Dec3NElementParser();
}
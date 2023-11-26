using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

namespace VertexBufferParser;

public interface IElementWriter
{
    int WriteElement(Span<byte> elementSpan, Span<char> destination, IFormatProvider? formatProvider = null);
}

public class ElementWriter<T> : IElementWriter where T : unmanaged, ISpanFormattable
{
    private readonly int _count;
    private readonly string _itemSeparator;
    private readonly string? _format;

    public ElementWriter(int? count = null, string? itemSeparator = null, string? format = null)
    {
        _count = count ?? int.MaxValue;
        _itemSeparator = itemSeparator ?? " ";
        _format = format;
    }

    public virtual int WriteElement(Span<byte> elementSpan, Span<char> destination, IFormatProvider? formatProvider = null)
    {
        Span<T> element = MemoryMarshal.Cast<byte, T>(elementSpan);

        int length = 0;

        var format = _format;

        for (int i = 0; i < element.Length; i++)
        {
            var item = element[i];
            _ = item.TryFormat(destination.Slice(length), out int charsWritten, format, formatProvider ??= NumberFormatInfo.InvariantInfo);

            length += charsWritten;

            if (i < element.Length - 1)
            {
                // Copy the separator and increase the length
                _itemSeparator.CopyTo(destination.Slice(length, _itemSeparator.Length));
                length += _itemSeparator.Length;
            }         
        }

        return length;
    }
}

public class Dec3NElementWriter : ElementWriter<float>
{
    public Dec3NElementWriter(string? itemSeparator = null, string? format = null)
        : base(3, itemSeparator, format)
    {     
    }

    public override int WriteElement(Span<byte> elementSpan, Span<char> destination, IFormatProvider? formatProvider = null)
    {
        // Get the Dec3N
        var dec3N = MemoryMarshal.Read<Dec3N>(elementSpan);
        
        // Convert to 3 floats
        Span<float> floats = stackalloc float[3] { dec3N.X, dec3N.Y, dec3N.Z };
        var floatsSpan = MemoryMarshal.Cast<float, byte>(floats);

        // Write as 3 floats
        return base.WriteElement(floatsSpan, destination, formatProvider);
    }
}
using System.Diagnostics;
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

    public int WriteElement(Span<byte> elementSpan, Span<char> destination, IFormatProvider? formatProvider = null)
    {
        Span<T> element = MemoryMarshal.Cast<byte, T>(elementSpan);

        Debug.Assert(element.Length == _count);

        int length = 0;

        var format = _format;

        for (int i = 0; i < element.Length; i++)
        {
            var item = element[i];
            _ = item.TryFormat(destination.Slice(length), out int charsWritter, format ?? default, formatProvider);

            length += charsWritter;

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
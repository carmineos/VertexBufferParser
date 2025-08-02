using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VertexBufferParser;

public readonly ref struct VertexElementAccessor
{
    private readonly VertexSpan _vertices;
    private readonly ElementDescriptor[] _elements;

    public VertexElementAccessor(VertexSpan vertices, ElementDescriptor[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        
        _vertices = vertices;
        _elements = elements;
    }

    public int VertexCount => _vertices.Length;
    public int ElementCount => _elements.Length;

    private Span<byte> GetElement(int vertexIndex, ElementDescriptor desc)
    {
        var vertex = _vertices[vertexIndex];
        if (desc.Offset < 0 || desc.Offset + desc.Size > vertex.Length)
            throw new ArgumentOutOfRangeException(nameof(desc));
        return vertex.Slice(desc.Offset, desc.Size);
    }

    public Span<byte> GetElement(int vertexIndex, int elementIndex)
    {
        if ((uint)elementIndex >= (uint)_elements.Length)
            throw new ArgumentOutOfRangeException(nameof(elementIndex));
        return GetElement(vertexIndex, _elements[elementIndex]);
    }

    private ref T GetElement<T>(int vertexIndex, ElementDescriptor desc) where T : unmanaged
    {
        var span = GetElement(vertexIndex, desc);
        if (span.Length < Unsafe.SizeOf<T>())
            throw new InvalidOperationException($"Element size is smaller than size of {typeof(T).Name}.");
        return ref System.Runtime.InteropServices.MemoryMarshal.AsRef<T>(span);
    }

    public ref T GetElement<T>(int vertexIndex, int elementIndex) where T : unmanaged
    {
        if ((uint)elementIndex >= (uint)_elements.Length)
            throw new ArgumentOutOfRangeException(nameof(elementIndex));
        return ref GetElement<T>(vertexIndex, _elements[elementIndex]);
    }
    
    public ref T GetElement<T>(int vertexIndex, ElementDescriptorName name) where T : unmanaged
    {
        var desc = ElementDescriptorExtensions.GetDescriptor(_elements, name);
        return ref GetElement<T>(vertexIndex, desc);
    }

    private VertexElementSpan<T> GetElementSpan<T>(ElementDescriptor desc) where T : unmanaged
    {
        return new VertexElementSpan<T>(_vertices, desc);
    }
    
    public VertexElementSpan<T> GetElementSpan<T>(ElementDescriptorName name) where T : unmanaged
    {
        var desc = ElementDescriptorExtensions.GetDescriptor(_elements, name);
        return GetElementSpan<T>(desc);
    }
}
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VertexBufferParser;

public readonly ref struct VertexElementSpan<T> where T : unmanaged
{
    private readonly VertexSpan _vertices;
    private readonly ElementDescriptor _descriptor;

    public VertexElementSpan(VertexSpan vertices, ElementDescriptor descriptor)
    {
        if (descriptor.Size != Unsafe.SizeOf<T>())
            throw new ArgumentException($"Descriptor size does not match size of {typeof(T).Name}.");
        
        _vertices = vertices;
        _descriptor = descriptor;
    }

    public int Length => _vertices.Length;

    public ref T this[int index]
    {
        get
        {
            var vertex = _vertices[index];
            var elementSpan = vertex.Slice(_descriptor.Offset, _descriptor.Size);
            return ref MemoryMarshal.AsRef<T>(elementSpan);
        }
    }
}
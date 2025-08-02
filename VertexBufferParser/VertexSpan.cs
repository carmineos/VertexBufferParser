namespace VertexBufferParser;

public readonly ref struct VertexSpan
{
    private readonly Span<byte> _buffer;
    private readonly int _vertexSize;

    public VertexSpan(Span<byte> buffer, int vertexSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(vertexSize);
            
        if (buffer.Length % vertexSize != 0)
            throw new ArgumentException("Buffer length must be divisible by vertex size.", nameof(buffer));

        _buffer = buffer;
        _vertexSize = vertexSize;
    }

    public int Length => _buffer.Length / _vertexSize;

    public Span<byte> this[int index]
    {
        get
        {
            if ((uint)index >= (uint)Length)
                throw new ArgumentOutOfRangeException(nameof(index));
                
            return _buffer.Slice(index * _vertexSize, _vertexSize);
        }
    }
}
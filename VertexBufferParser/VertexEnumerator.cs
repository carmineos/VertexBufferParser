namespace VertexBufferParser;

using System;

internal readonly ref struct VerticesEnumerator
{
    private readonly ReadOnlySpan<char> _buffer;
    private readonly string _verticesSeparator;

    public VerticesEnumerator(ReadOnlySpan<char> buffer, string verticesSeparator)
    {
        _buffer = buffer;
        _verticesSeparator = verticesSeparator;
    }

    public Enumerator GetEnumerator() => new Enumerator(_buffer, _verticesSeparator);

    public ref struct Enumerator
    {
        private ReadOnlySpan<char> _remaining;
        private ReadOnlySpan<char> _current;
        private readonly string _separator;

        public Enumerator(ReadOnlySpan<char> buffer, string separator)
        {
            _remaining = buffer;
            _current = default;
            _separator = separator;
        }

        public ReadOnlySpan<char> Current => _current;

        public bool MoveNext()
        {
            if (_remaining.IsEmpty)
                return false;

            int sepIndex;

            // TODO: Try to use .EnumerateLines where possible
            if (_separator == "\n" || _separator == "\r\n")
            {
                sepIndex = _remaining.IndexOfAny('\r', '\n');
            }
            else
            {
                sepIndex = _remaining.IndexOf(_separator);
            }

            if (sepIndex < 0)
            {
                _current = _remaining;
                _remaining = ReadOnlySpan<char>.Empty;
                return true;
            }

            _current = _remaining[..sepIndex];

            if (_separator == "\r\n" && sepIndex + 1 < _remaining.Length && _remaining[sepIndex] == '\r' && _remaining[sepIndex + 1] == '\n')
                _remaining = _remaining[(sepIndex + 2)..];
            else
                _remaining = _remaining[(sepIndex + _separator.Length)..];

            return true;
        }
    }
}
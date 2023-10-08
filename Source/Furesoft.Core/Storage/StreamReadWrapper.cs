﻿namespace Furesoft.Core.Storage;

/// <summary>
///     Wrapper around a Stream, for reading only.
///     This allows client to limit a stream to a particular length.
/// </summary>
public class StreamReadWrapper : Stream
{
    private readonly Stream _parent;
    private long _position;
    private readonly long _readLimit;

    public StreamReadWrapper(Stream target, long readLimit)
    {
        _parent = target;
        _readLimit = readLimit;
    }

    public override long Position
    {
        get => _position;
        set => throw new NotImplementedException();
    }

    public override long Length => _readLimit;

    public override bool CanRead => _parent.CanRead;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override void Flush()
    {
        // nothing to do here
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (_readLimit - _position == 0)
            // end of stream reached; Return 0
            return 0;

        var read = _parent.Read(buffer, offset, (int) Math.Min(count, _readLimit - _position));
        _position += read;
        return read;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotImplementedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }
}
using System;
using System.IO;

namespace Furesoft.Core.Storage
{
	public class Block : IBlock
	{
		private readonly byte[] _firstSector;
		private readonly long?[] _cachedHeaderValue = new long?[5];
		private readonly Stream _stream;
		private readonly BlockStorage _storage;
		private bool _isFirstSectorDirty;
		private bool _isDisposed;

		public event EventHandler Disposed;

		public uint Id { get; }

		public Block(BlockStorage storage, uint id, byte[] firstSector, Stream stream)
		{
			if (firstSector == null)
				throw new ArgumentNullException(nameof(firstSector));

			if (firstSector.Length != storage.DiskSectorSize)
				throw new ArgumentException("firstSector length must be " + storage.DiskSectorSize);

			_storage = storage;
			Id = id;
			_stream = stream ?? throw new ArgumentNullException("stream");
			_firstSector = firstSector;
		}

		public long GetHeader(int field)
		{
			if (_isDisposed)
			{
				throw new ObjectDisposedException("Block");
			}

			// Validate field number
			if (field < 0)
			{
				throw new IndexOutOfRangeException();
			}
			if (field >= (_storage.BlockHeaderSize / 8))
			{
				throw new ArgumentException("Invalid field: " + field);
			}

			// Check from cache, if it is there then return it
			if (field < _cachedHeaderValue.Length)
			{
				if (_cachedHeaderValue[field] == null)
				{
					_cachedHeaderValue[field] = BufferHelper.ReadBufferInt64(_firstSector, field * 8);
				}
				return (long)_cachedHeaderValue[field];
			}
			// Otherwise return straight away
			else
			{
				return BufferHelper.ReadBufferInt64(_firstSector, field * 8);
			}
		}

		public void SetHeader(int field, long value)
		{
			if (_isDisposed)
			{
				throw new ObjectDisposedException("Block");
			}

			if (field < 0)
			{
				throw new IndexOutOfRangeException();
			}

			// Update cache if this field is cached
			if (field < _cachedHeaderValue.Length)
			{
				_cachedHeaderValue[field] = value;
			}

			// Write in cached buffer
			BufferHelper.WriteBuffer((long)value, _firstSector, field * 8);
			_isFirstSectorDirty = true;
		}

		public void Read(byte[] dest, int destOffset, int srcOffset, int count)
		{
			if (_isDisposed)
			{
				throw new ObjectDisposedException("Block");
			}

			// Validate argument
			if (!((count >= 0) && ((count + srcOffset) <= _storage.BlockContentSize)))
			{
				throw new ArgumentOutOfRangeException("Requested count is outside of src bounds: Count=" + count, "count");
			}

			if (!((count + destOffset) <= dest.Length))
			{
				throw new ArgumentOutOfRangeException("Requested count is outside of dest bounds: Count=" + count);
			}

			// If part of remain data belongs to the firstSector buffer
			// then copy from the firstSector first
			var dataCopied = 0;
			var copyFromFirstSector = (_storage.BlockHeaderSize + srcOffset) < _storage.DiskSectorSize;
			if (copyFromFirstSector)
			{
				var tobeCopied = Math.Min(_storage.DiskSectorSize - _storage.BlockHeaderSize - srcOffset, count);

				Buffer.BlockCopy(src: _firstSector
					, srcOffset: _storage.BlockHeaderSize + srcOffset
					, dst: dest
					, dstOffset: destOffset
					, count: tobeCopied);

				dataCopied += tobeCopied;
			}

			// Move the stream to correct position,
			// if there is still some data tobe copied
			if (dataCopied < count)
			{
				if (copyFromFirstSector)
				{
					_stream.Position = (Id * _storage.BlockSize) + _storage.DiskSectorSize;
				}
				else
				{
					_stream.Position = (Id * _storage.BlockSize) + _storage.BlockHeaderSize + srcOffset;
				}
			}

			// Start copying until all data required is copied
			while (dataCopied < count)
			{
				var bytesToRead = Math.Min(_storage.DiskSectorSize, count - dataCopied);
				var thisRead = _stream.Read(dest, destOffset + dataCopied, bytesToRead);
				if (thisRead == 0)
				{
					throw new EndOfStreamException();
				}
				dataCopied += thisRead;
			}
		}

		public void Write(byte[] src, int srcOffset, int dstOffset, int count)
		{
			if (_isDisposed)
			{
				throw new ObjectDisposedException("Block");
			}

			// Validate argument
			if (!((dstOffset >= 0) && ((dstOffset + count) <= _storage.BlockContentSize)))
			{
				throw new ArgumentOutOfRangeException("Count argument is outside of dest bounds: Count=" + count
					, "count");
			}

			if (!((srcOffset >= 0) && ((srcOffset + count) <= src.Length)))
			{
				throw new ArgumentOutOfRangeException("Count argument is outside of src bounds: Count=" + count
					, "count");
			}

			// Write bytes that belong to the firstSector
			if ((_storage.BlockHeaderSize + dstOffset) < _storage.DiskSectorSize)
			{
				var thisWrite = Math.Min(count, _storage.DiskSectorSize - _storage.BlockHeaderSize - dstOffset);
				Buffer.BlockCopy(src: src
					, srcOffset: srcOffset
					, dst: _firstSector
					, dstOffset: _storage.BlockHeaderSize + dstOffset
					, count: thisWrite);
				_isFirstSectorDirty = true;
			}

			// Write bytes that do not belong to the firstSector
			if ((_storage.BlockHeaderSize + dstOffset + count) > _storage.DiskSectorSize)
			{
				// Move underlying stream to correct position ready for writting
				_stream.Position = (Id * _storage.BlockSize)
					+ Math.Max(_storage.DiskSectorSize, _storage.BlockHeaderSize + dstOffset);

				// Exclude bytes that have been written to the first sector
				var d = _storage.DiskSectorSize - (_storage.BlockHeaderSize + dstOffset);
				if (d > 0)
				{
					dstOffset += d;
					srcOffset += d;
					count -= d;
				}

				// Keep writing until all data is written
				var written = 0;
				while (written < count)
				{
					var bytesToWrite = (int)Math.Min(4096, count - written);
					_stream.Write(src, srcOffset + written, bytesToWrite);
					_stream.Flush();
					written += bytesToWrite;
				}
			}
		}

		public override string ToString()
		{
			return string.Format("[Block: Id={0}, ContentLength={1}, Prev={2}, Next={3}]"
				, Id
				, GetHeader(2)
				, GetHeader(3)
				, GetHeader(0));
		}

		protected virtual void OnDisposed(EventArgs e)
		{
			Disposed?.Invoke(this, e);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing && !_isDisposed)
			{
				_isDisposed = true;

				if (_isFirstSectorDirty)
				{
					_stream.Position = (Id * _storage.BlockSize);
					_stream.Write(_firstSector, 0, 4096);
					_stream.Flush();
					_isFirstSectorDirty = false;
				}

				OnDisposed(EventArgs.Empty);
			}
		}

		~Block()
		{
			Dispose(false);
		}
	}
}
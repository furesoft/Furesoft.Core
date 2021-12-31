using System;

namespace Furesoft.Core.ObjectDB.IO
{
	/// <summary>
	///   The interface for buffered IO
	/// </summary>
	public interface IMultiBufferedFileIO : IDisposable
	{
		long Length { get; }

		long CurrentPosition { get; }

		void SetCurrentWritePosition(long currentPosition);

		void SetCurrentReadPosition(long currentPosition);

		void WriteByte(byte b);

		byte ReadByte();

		void WriteBytes(byte[] bytes);

		byte[] ReadBytes(int size);

		void FlushAll();

		void Close();
	}
}
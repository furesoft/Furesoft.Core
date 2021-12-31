using System;

namespace Furesoft.Core.Storage.Serializers
{
	public class TreeIntSerializer : ISerializer<int>
	{
		public byte[] Serialize(int value)
		{
			return LittleEndianByteOrder.GetBytes(value);
		}

		public int Deserialize(byte[] buffer, int offset, int length)
		{
			if (length != 4)
			{
				throw new ArgumentException("Invalid length: " + length);
			}

			return BufferHelper.ReadBufferInt32(buffer, offset);
		}

		public bool IsFixedSize
		{
			get
			{
				return true;
			}
		}

		public int Length
		{
			get
			{
				return 4;
			}
		}
	}
}
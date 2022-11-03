﻿namespace Furesoft.Core.Storage;

	public interface ISerializer<K>
	{
		byte[] Serialize(K value);

		K Deserialize(byte[] buffer, int offset, int length);

		bool IsFixedSize
		{
			get;
		}

		int Length
		{
			get;
		}
	}
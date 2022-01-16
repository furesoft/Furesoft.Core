using System;
using System.Collections.Generic;
using System.IO;

namespace Furesoft.Core.Storage
{
	public class BlockStorage : IBlockStorage
	{
		private readonly Stream stream;
		private readonly Dictionary<uint, Block> blocks = new();

		public int DiskSectorSize { get; }

		public int BlockSize { get; }

		public int BlockHeaderSize { get; }

		public int BlockContentSize { get; }

		public BlockStorage(Stream storage, int blockSize = 40960, int blockHeaderSize = 48)
		{
			if (blockHeaderSize >= blockSize)
			{
				throw new ArgumentException("blockHeaderSize cannot be " +
					"larger than or equal " +
					"to " + "blockSize");
			}

			if (blockSize < 128)
			{
				throw new ArgumentException("blockSize too small");
			}

			DiskSectorSize = (blockSize >= 4096) ? 4096 : 128;
			BlockSize = blockSize;
			BlockHeaderSize = blockHeaderSize;
			BlockContentSize = blockSize - blockHeaderSize;
			stream = storage ?? throw new ArgumentNullException(nameof(storage));
		}

		public IBlock Find(uint blockId)
		{
			// Check from initialized blocks
			if (blocks.ContainsKey(blockId))
			{
				return blocks[blockId];
			}

			// First, move to that block.
			// If there is no such block return NULL
			var blockPosition = blockId * BlockSize;
			if ((blockPosition + BlockSize) > stream.Length)
			{
				return null;
			}

			// Read the first 4KB of the block to construct a block from it
			var firstSector = new byte[DiskSectorSize];
			stream.Position = blockId * BlockSize;
			stream.Read(firstSector, 0, DiskSectorSize);

			var block = new Block(this, blockId, firstSector, stream);
			OnBlockInitialized(block);
			return block;
		}

		public IBlock CreateNew()
		{
			if ((stream.Length % BlockSize) != 0)
			{
				throw new DataMisalignedException("Unexpected length of the stream: " + stream.Length);
			}

			// Calculate new block id
			var blockId = (uint)Math.Ceiling((double)stream.Length / (double)BlockSize);

			// Extend length of underlying stream
			stream.SetLength((long)((blockId * BlockSize) + BlockSize));
			stream.Flush();

			// Return desired block
			var block = new Block(this, blockId, new byte[DiskSectorSize], stream);
			OnBlockInitialized(block);
			return block;
		}

		protected virtual void OnBlockInitialized(Block block)
		{
			// Keep reference to it
			blocks[block.Id] = block;

			// When block disposed, remove it from memory
			block.Disposed += HandleBlockDisposed;
		}

		protected virtual void HandleBlockDisposed(object sender, EventArgs e)
		{
			// Stop listening to it
			var block = (Block)sender;
			block.Disposed -= HandleBlockDisposed;

			// Remove it from memory
			blocks.Remove(block.Id);
		}
	}
}
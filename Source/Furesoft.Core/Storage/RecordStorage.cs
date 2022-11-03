using System;
using System.Collections.Generic;
using System.IO;

namespace Furesoft.Core.Storage;

	/// <summary>
	/// Record storage service that store data in form of records, each
	/// record made up from one or several blocks
	/// </summary>
	public class RecordStorage : IRecordStorage
	{
		private readonly IBlockStorage storage;
		private const int MaxRecordSize = 4194304; // 4MB
		private const int KNextBlockId = 0;
		private const int KRecordLength = 1;
		private const int KBlockContentLength = 2;
		private const int KPreviousBlockId = 3;
		private const int KIsDeleted = 4;

		public RecordStorage(IBlockStorage storage)
		{
			this.storage = storage ?? throw new ArgumentNullException(nameof(storage));

			if (storage.BlockHeaderSize < 48)
			{
				throw new ArgumentException("Record storage needs at least 48 header bytes");
			}
		}

		public virtual byte[] Find(uint recordId)
		{
			// First grab the block
			using var block = storage.Find(recordId);
			if (block == null)
			{
				return null;
			}

			// If this is a deleted block then ignore it
			if (block.GetHeader(KIsDeleted) == 1L)
			{
				return null;
			}

			// If this block is a child block then also ignore it
			if (block.GetHeader(KPreviousBlockId) != 0L)
			{
				return null;
			}

			// Grab total record size and allocate coressponded memory
			var totalRecordSize = block.GetHeader(KRecordLength);
			if (totalRecordSize > MaxRecordSize)
			{
				throw new NotSupportedException("Unexpected record length: " + totalRecordSize);
			}
			var data = new byte[totalRecordSize];
			var bytesRead = 0;

			// Now start filling data
			var currentBlock = block;
			while (true)
			{
				uint nextBlockId;

				using (currentBlock)
				{
					var thisBlockContentLength = currentBlock.GetHeader(KBlockContentLength);
					if (thisBlockContentLength > storage.BlockContentSize)
					{
						throw new InvalidDataException("Unexpected block content length: " + thisBlockContentLength);
					}

					// Read all available content of current block
					currentBlock.Read(dst: data, dstOffset: bytesRead, srcOffset: 0, count: (int)thisBlockContentLength);

					// Update number of bytes read
					bytesRead += (int)thisBlockContentLength;

					// Move to the next block if there is any
					nextBlockId = (uint)currentBlock.GetHeader(KNextBlockId);
					if (nextBlockId == 0)
					{
						return data;
					}
				}

				currentBlock = storage.Find(nextBlockId);
				if (currentBlock == null)
				{
					throw new InvalidDataException("Block not found by id: " + nextBlockId);
				}
			}
		}

		public virtual uint Create(Func<uint, byte[]> dataGenerator)
		{
			if (dataGenerator == null)
			{
				throw new ArgumentException();
			}

			using var firstBlock = AllocateBlock();
			var returnId = firstBlock.Id;

			// Alright now begin writing data
			var data = dataGenerator(returnId);
			var dataWritten = 0;
			var dataTobeWritten = data.Length;
			firstBlock.SetHeader(KRecordLength, dataTobeWritten);

			// If no data tobe written,
			// return this block straight away
			if (dataTobeWritten == 0)
			{
				return returnId;
			}

			// Otherwise continue to write data until completion
			var currentBlock = firstBlock;
			while (dataWritten < dataTobeWritten)
			{
				IBlock nextBlock = null;

				using (currentBlock)
				{
					// Write as much as possible to this block
					var thisWrite = (int)Math.Min(storage.BlockContentSize, dataTobeWritten - dataWritten);
					currentBlock.Write(data, dataWritten, 0, thisWrite);
					currentBlock.SetHeader(KBlockContentLength, (long)thisWrite);
					dataWritten += thisWrite;

					// If still there are data to be written,
					// move to the next block
					if (dataWritten < dataTobeWritten)
					{
						nextBlock = AllocateBlock();
						var success = false;
						try
						{
							nextBlock.SetHeader(KPreviousBlockId, currentBlock.Id);
							currentBlock.SetHeader(KNextBlockId, nextBlock.Id);
							success = true;
						}
						finally
						{
							if (!success && nextBlock != null)
							{
								nextBlock.Dispose();
								nextBlock = null;
							}
						}
					}
					else
					{
						break;
					}
				}

				// Move to the next block if possible
				if (nextBlock != null)
				{
					currentBlock = nextBlock;
				}
			}

			// return id of the first block that got dequeued
			return returnId;
		}

		public virtual uint Create(byte[] data)
		{
			if (data == null)
			{
				throw new ArgumentException();
			}

			return Create(_ => data);
		}

		public virtual uint Create()
		{
			using var firstBlock = AllocateBlock();
			return firstBlock.Id;
		}

		public virtual void Delete(uint recordId)
		{
			using var block = storage.Find(recordId);
			var currentBlock = block;
			while (true)
			{
				IBlock nextBlock = null;

				using (currentBlock)
				{
					MarkAsFree(currentBlock.Id);
					currentBlock.SetHeader(KIsDeleted, 1L);

					var nextBlockId = (uint)currentBlock.GetHeader(KNextBlockId);
					if (nextBlockId == 0)
					{
						break;
					}
					else
					{
						nextBlock = storage.Find(nextBlockId);
						if (currentBlock == null)
						{
							throw new InvalidDataException("Block not found by id: " + nextBlockId);
						}
					}
				}

				// Move to next block
				if (nextBlock != null)
				{
					currentBlock = nextBlock;
				}
			}
		}

		public virtual void Update(uint recordId, byte[] data)
		{
			var written = 0;
			var total = data.Length;
			var blocks = FindBlocks(recordId);
			var blocksUsed = 0;
			var previousBlock = (IBlock)null;

			try
			{
				// Start writing block by block..
				while (written < total)
				{
					// Bytes to be written in this block
					var bytesToWrite = Math.Min(total - written, storage.BlockContentSize);

					// Get the block where the first byte of remaining data will be written to
					var blockIndex = (int)Math.Floor((double)written / (double)storage.BlockContentSize);

					// Find the block to write to:
					// If `blockIndex` exists in `blocks`, then write into it,
					// otherwise allocate a new one for writting
					var target = (IBlock)null;
					if (blockIndex < blocks.Count)
					{
						target = blocks[blockIndex];
					}
					else
					{
						target = AllocateBlock();
						if (target == null)
						{
							throw new Exception("Failed to allocate new block");
						}
						blocks.Add(target);
					}

					// Link with previous block
					if (previousBlock != null)
					{
						previousBlock.SetHeader(KNextBlockId, target.Id);
						target.SetHeader(KPreviousBlockId, previousBlock.Id);
					}

					// Write data
					target.Write(src: data, srcOffset: written, dstOffset: 0, count: bytesToWrite);
					target.SetHeader(KBlockContentLength, bytesToWrite);
					target.SetHeader(KNextBlockId, 0);
					if (written == 0)
					{
						target.SetHeader(KRecordLength, total);
					}

					// Get ready fr next loop
					blocksUsed++;
					written += bytesToWrite;
					previousBlock = target;
				}

				// After writing, delete off any unused blocks
				if (blocksUsed < blocks.Count)
				{
					for (var i = blocksUsed; i < blocks.Count; i++)
					{
						MarkAsFree(blocks[i].Id);
					}
				}
			}
			finally
			{
				// Always dispose all fetched blocks after finish using them
				foreach (var block in blocks)
				{
					block.Dispose();
				}
			}
		}

		/// <summary>
		/// Find all blocks of given record, return these blocks in order.
		/// </summary>
		/// <param name="recordId">Record identifier.</param>
		private List<IBlock> FindBlocks(uint recordId)
		{
			var blocks = new List<IBlock>();
			var success = false;

			try
			{
				var currentBlockId = recordId;

				do
				{
					// Grab next block
					var block = storage.Find(currentBlockId);
					if (block == null)
					{
						// Special case: if block #0 never created, then attempt to create it
						if (currentBlockId == 0)
						{
							block = storage.CreateNew();
						}
						else
						{
							throw new Exception("Block not found by id: " + currentBlockId);
						}
					}
					blocks.Add(block);

					// If this is a deleted block then ignore the fuck out of it
					if (1L == block.GetHeader(KIsDeleted))
					{
						throw new InvalidDataException("Block not found: " + currentBlockId);
					}

					// Move next
					currentBlockId = (uint)block.GetHeader(KNextBlockId);
				} while (currentBlockId != 0);

				success = true;
				return blocks;
			}
			finally
			{
				// Incase shit happens, dispose all fetched blocks
				if (!success)
				{
					foreach (var block in blocks)
					{
						block.Dispose();
					}
				}
			}
		}

		/// <summary>
		/// Allocate new block for use, either by dequeueing an exising non-used block
		/// or creating a new one
		/// </summary>
		/// <returns>Newly allocated block ready to use.</returns>
		private IBlock AllocateBlock()
		{
			IBlock newBlock;
			if (!TryFindFreeBlock(out var resuableBlockId))
			{
				newBlock = storage.CreateNew();
				if (newBlock == null)
				{
					throw new Exception("Failed to create new block");
				}
			}
			else
			{
				newBlock = storage.Find(resuableBlockId);
				if (newBlock == null)
				{
					throw new InvalidDataException("Block not found by id: " + resuableBlockId);
				}
				newBlock.SetHeader(KBlockContentLength, 0L);
				newBlock.SetHeader(KNextBlockId, 0L);
				newBlock.SetHeader(KPreviousBlockId, 0L);
				newBlock.SetHeader(KRecordLength, 0L);
				newBlock.SetHeader(KIsDeleted, 0L);
			}
			return newBlock;
		}

		private bool TryFindFreeBlock(out uint blockId)
		{
			blockId = 0;
			GetSpaceTrackingBlock(out var lastBlock, out var secondLastBlock);

			using (lastBlock)
			using (secondLastBlock)
			{
				// If this block is empty, then goto previous block
				var currentBlockContentLength = lastBlock.GetHeader(KBlockContentLength);
				if (currentBlockContentLength == 0)
				{
					// If there is no previous block, return false to indicate we can't dequeu
					if (secondLastBlock == null)
					{
						return false;
					}

					// Dequeue an uint from previous block, then mark current block as free
					blockId = ReadUInt32FromTrailingContent(secondLastBlock);

					// Back off 4 bytes before calling AppendUInt32ToContent
					secondLastBlock.SetHeader(KBlockContentLength, secondLastBlock.GetHeader(KBlockContentLength) - 4);
					AppendUInt32ToContent(secondLastBlock, lastBlock.Id);

					// Forward 4 bytes, as an uint32 has been written
					secondLastBlock.SetHeader(KBlockContentLength, secondLastBlock.GetHeader(KBlockContentLength) + 4);
					secondLastBlock.SetHeader(KNextBlockId, 0);
					lastBlock.SetHeader(KPreviousBlockId, 0);

					// Indicate success
					return true;
				}
				// If this block is not empty then dequeue an UInt32 from it
				else
				{
					blockId = ReadUInt32FromTrailingContent(lastBlock);
					lastBlock.SetHeader(KBlockContentLength, currentBlockContentLength - 4);

					// Indicate sucess
					return true;
				}
			}
		}

		private void AppendUInt32ToContent(IBlock block, uint value)
		{
			var contentLength = block.GetHeader(KBlockContentLength);

			if ((contentLength % 4) != 0)
			{
				throw new DataMisalignedException("Block content length not %4: " + contentLength);
			}

			block.Write(src: LittleEndianByteOrder.GetBytes(value), srcOffset: 0, dstOffset: (int)contentLength, count: 4);
		}

		private uint ReadUInt32FromTrailingContent(IBlock block)
		{
			var buffer = new byte[4];
			var contentLength = block.GetHeader(KBlockContentLength);

			if ((contentLength % 4) != 0)
			{
				throw new DataMisalignedException("Block content length not %4: " + contentLength);
			}

			if (contentLength == 0)
			{
				throw new InvalidDataException("Trying to dequeue UInt32 from an empty block");
			}

			block.Read(dst: buffer, dstOffset: 0, srcOffset: (int)contentLength - 4, count: 4);
			return LittleEndianByteOrder.GetUInt32(buffer);
		}

		private void MarkAsFree(uint blockId)
		{
			IBlock targetBlock = null;
			GetSpaceTrackingBlock(out var lastBlock, out var secondLastBlock);

			using (lastBlock)
			using (secondLastBlock)
			{
				try
				{
					// Just append a number, if there is some space left
					var contentLength = lastBlock.GetHeader(KBlockContentLength);
					if (contentLength + 4 <= storage.BlockContentSize)
					{
						targetBlock = lastBlock;
					}
					// No more fucking space left, allocate new block for writing.
					// Note that we allocate fresh new block, if we reuse it may fuck things up
					else
					{
						targetBlock = storage.CreateNew();
						targetBlock.SetHeader(KPreviousBlockId, lastBlock.Id);

						lastBlock.SetHeader(KNextBlockId, targetBlock.Id);

						contentLength = 0;
					}

					// Write!
					AppendUInt32ToContent(targetBlock, blockId);

					// Extend the block length to 4, as we wrote a number
					targetBlock.SetHeader(KBlockContentLength, contentLength + 4);
				}
				finally
				{
					// Always dispose targetBlock
					targetBlock?.Dispose();
				}
			}
		}

		/// <summary>
		/// Get the last 2 blocks from the free space tracking record,
		/// </summary>
		private void GetSpaceTrackingBlock(out IBlock lastBlock, out IBlock secondLastBlock)
		{
			lastBlock = null;
			secondLastBlock = null;

			// Grab all record 0's blocks
			var blocks = FindBlocks(0);

			try
			{
				if (blocks == null || (blocks.Count == 0))
				{
					throw new Exception("Failed to find blocks of record 0");
				}

				// Assign
				lastBlock = blocks[blocks.Count - 1];
				if (blocks.Count > 1)
				{
					secondLastBlock = blocks[^2];
				}
			}
			finally
			{
				// Awlays dispose unused blocks
				if (blocks != null)
				{
					foreach (var block in blocks)
					{
						if (lastBlock == null || block != lastBlock
							&& (secondLastBlock == null || block != secondLastBlock))
						{
							block.Dispose();
						}
					}
				}
			}
		}
	}
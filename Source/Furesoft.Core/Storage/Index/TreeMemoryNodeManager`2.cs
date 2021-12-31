using System;
using System.Collections.Generic;

namespace Furesoft.Core.Storage.Index
{
	public class TreeMemoryNodeManager<K, V> : ITreeNodeManager<K, V>
	{
		private readonly Dictionary<uint, TreeNode<K, V>> _nodes = new Dictionary<uint, TreeNode<K, V>>();
		private int _idCounter = 1;

		public IComparer<Tuple<K, V>> EntryComparer { get; }

		public ushort MinEntriesPerNode { get; }

		public IComparer<K> KeyComparer { get; }

		public TreeNode<K, V> RootNode { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Sdb.BTree.MemoryNodeManager`2"/> class.
		/// </summary>
		/// <param name="minEntriesCountPerNode">This multiply by 2 is the degree of the tree</param>
		/// <param name="keyComparer">Key comparer.</param>
		public TreeMemoryNodeManager(ushort minEntriesCountPerNode, IComparer<K> keyComparer)
		{
			KeyComparer = keyComparer;
			EntryComparer = Comparer<Tuple<K, V>>.Create((t1, t2) => KeyComparer.Compare(t1.Item1, t2.Item1));
			MinEntriesPerNode = minEntriesCountPerNode;
			RootNode = Create(null, null);
		}

		public TreeNode<K, V> Create(IEnumerable<Tuple<K, V>> entries, IEnumerable<uint> childrenIds)
		{
			var newNode = new TreeNode<K, V>(this
				, (uint)(_idCounter++)
				, 0
				, entries
				, childrenIds);

			_nodes[newNode.Id] = newNode;

			return newNode;
		}

		public TreeNode<K, V> Find(uint id)
		{
			if (!_nodes.ContainsKey(id))
			{
				throw new ArgumentException("Node not found by id: " + id);
			}

			return _nodes[id];
		}

		public TreeNode<K, V> CreateNewRoot(K key, V value, uint leftNodeId, uint rightNodeId)
		{
			var newNode = Create(new Tuple<K, V>[] { new Tuple<K, V>(key, value) }
				, new uint[] { leftNodeId, rightNodeId }
			);
			RootNode = newNode;
			return newNode;
		}

		public void Delete(TreeNode<K, V> target)
		{
			if (target == RootNode)
			{
				RootNode = null;
			}
			if (_nodes.ContainsKey(target.Id))
			{
				_nodes.Remove(target.Id);
			}
		}

		public void MakeRoot(TreeNode<K, V> target)
		{
			RootNode = target;
		}

		public void MarkAsChanged(TreeNode<K, V> node)
		{
			// does nothing
		}

		public void SaveChanges()
		{
			// does nothing
		}
	}
}
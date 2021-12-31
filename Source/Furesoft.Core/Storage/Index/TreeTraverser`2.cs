using System;
using System.Collections;
using System.Collections.Generic;

namespace Furesoft.Core.Storage.Index
{
	public class TreeTraverser<K, V> : IEnumerable<Tuple<K, V>>
	{
		private readonly TreeNode<K, V> fromNode;
		private readonly int _fromIndex;
		private readonly TreeTraverseDirection _direction;
		private readonly ITreeNodeManager<K, V> _nodeManager;

		/// <summary>
		/// Initializes a new instance of the <see cref="Sdb.BTree.TreeTraverser`2"/> class.
		/// </summary>
		/// <param name="nodeManager">Node manager.</param>
		/// <param name="fromNode">From node.</param>
		/// <param name="fromIndex">From index.</param>
		/// <param name="direction">Direction.</param>
		public TreeTraverser(ITreeNodeManager<K, V> nodeManager
			, TreeNode<K, V> fromNode
			, int fromIndex
			, TreeTraverseDirection direction)
		{
			if (fromNode == null)
				throw new ArgumentNullException("fromNode");

			_direction = direction;
			_fromIndex = fromIndex;
			this.fromNode = fromNode;
			_nodeManager = nodeManager;
		}

		IEnumerator<Tuple<K, V>> IEnumerable<Tuple<K, V>>.GetEnumerator()
		{
			return new TreeEnumerator<K, V>(_nodeManager, fromNode, _fromIndex, _direction, _direction);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			// Use the generic version
			return ((IEnumerable<Tuple<K, V>>)this).GetEnumerator();
		}
	}
}
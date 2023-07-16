using System.Collections;
using Furesoft.Core.ObjectDB.Api;

namespace Furesoft.Core.ObjectDB.Btree;

	internal sealed class InMemoryBTreeMultipleValuesPerKey : AbstractBTree, IBTreeMultipleValuesPerKey
	{
		private static int nextId = 1;

		private int _id;

		public InMemoryBTreeMultipleValuesPerKey(int degree)
			: base(degree, new InMemoryPersister())
		{
			_id = nextId++;
		}

		#region IBTreeMultipleValuesPerKey Members

		public IList Search(IComparable key)
		{
			var theRoot = (IBTreeNodeMultipleValuesPerKey)GetRoot();
			return theRoot.Search(key);
		}

		public override IBTreeNode BuildNode()
		{
			return new InMemoryBTreeNodeMultipleValuesPerKey(this);
		}

		public override object GetId()
		{
			return _id;
		}

		public override void SetId(object id)
		{
			_id = (int)id;
		}

		public override void Clear()
		{
		}

		public override IEnumerator Iterator<T>(OrderByConstants orderBy)
		{
			return new BTreeIteratorMultipleValuesPerKey<T>(this, orderBy);
		}

		#endregion IBTreeMultipleValuesPerKey Members
	}
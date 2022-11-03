using System;
using System.Collections;
using Furesoft.Core.ObjectDB.Api;

namespace Furesoft.Core.ObjectDB.Btree;

	internal abstract class BTreeMultipleValuesPerKey : AbstractBTree, IBTreeMultipleValuesPerKey
	{
		protected BTreeMultipleValuesPerKey(int degree, IBTreePersister persister)
			: base(degree, persister)
		{
		}

		#region IBTreeMultipleValuesPerKey Members

		public virtual IList Search(IComparable key)
		{
			var theRoot = (IBTreeNodeMultipleValuesPerKey)GetRoot();
			return theRoot.Search(key);
		}

		public override IEnumerator Iterator<T>(OrderByConstants orderBy)
		{
			return new BTreeIteratorMultipleValuesPerKey<T>(this, orderBy);
		}

		public abstract override object GetId();

		public abstract override void SetId(object arg1);

		#endregion IBTreeMultipleValuesPerKey Members
	}
using System;
using System.Collections;
using Furesoft.Core.ObjectDB.Api;

namespace Furesoft.Core.ObjectDB.Btree
{
	internal abstract class BTreeSingleValuePerKey : AbstractBTree, IBTreeSingleValuePerKey
	{
		protected BTreeSingleValuePerKey(int degree, IBTreePersister persister)
			: base(degree, persister)
		{
		}

		#region IBTreeSingleValuePerKey Members

		public virtual object Search(IComparable key)
		{
			var theRoot = (IBTreeNodeOneValuePerKey)GetRoot();
			return theRoot.Search(key);
		}

		public override IEnumerator Iterator<T>(OrderByConstants orderBy)
		{
			return new BTreeIteratorSingleValuePerKey<T>(this, orderBy);
		}

		public abstract override object GetId();

		public abstract override void SetId(object arg1);

		#endregion IBTreeSingleValuePerKey Members
	}
}
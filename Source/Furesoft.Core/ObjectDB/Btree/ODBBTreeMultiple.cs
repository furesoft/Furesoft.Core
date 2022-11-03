using Furesoft.Core.ObjectDB.Api;

namespace Furesoft.Core.ObjectDB.Btree;

	/// <summary>
	///   The NDatabase ODB BTree.
	/// </summary>
	/// <remarks>
	///   The NDatabase ODB BTree. It extends the DefaultBTree implementation to add the ODB OID generated by the ODB database.
	/// </remarks>
	internal sealed class OdbBtreeMultiple : BTreeMultipleValuesPerKey
	{
		private OID _oid;

		public OdbBtreeMultiple(int degree, IBTreePersister persister) : base(degree, persister)
		{
		}

		public override IBTreeNode BuildNode()
		{
			return new OdbBtreeNodeMultiple(this);
		}

		public override object GetId()
		{
			return _oid;
		}

		public override void SetId(object id)
		{
			_oid = (OID)id;
		}
	}
using Furesoft.Core.ObjectDB.Api;
using Furesoft.Core.ObjectDB.Btree;
using Furesoft.Core.ObjectDB.Core.BTree;

namespace Furesoft.Core.ObjectDB.Core.Query.List;

	/// <summary>
	///   A collection using a BTtree as a back-end component.
	/// </summary>
	/// <remarks>
	///   A collection using a BTtree as a back-end component. Lazy because it only keeps the oids of the objects. When asked for an object, loads it on demand and returns it
	/// </remarks>
	internal sealed class LazyBTreeCollection<T> : AbstractBTreeCollection<T>
	{
		private readonly bool _returnObjects;
		private readonly IStorageEngine _storageEngine;

		public LazyBTreeCollection(IStorageEngine engine, bool returnObjects) : base(OrderByConstants.OrderByAsc)
		{
			_storageEngine = engine;
			_returnObjects = returnObjects;
		}

		protected override IBTree BuildTree(int degree)
		{
			return new InMemoryBTreeMultipleValuesPerKey(degree);
		}

		protected override IEnumerator<T> Iterator(OrderByConstants orderByType)
		{
			return new LazyOdbBtreeIteratorMultiple<T>(GetTree(), orderByType, _storageEngine, _returnObjects);
		}
	}
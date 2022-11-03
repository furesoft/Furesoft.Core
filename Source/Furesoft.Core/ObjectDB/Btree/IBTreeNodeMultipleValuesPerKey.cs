using System;
using System.Collections;

namespace Furesoft.Core.ObjectDB.Btree;

	/// <summary>
	///   The interface for btree nodes that accept One Value Per Key
	/// </summary>
	internal interface IBTreeNodeMultipleValuesPerKey : IBTreeNode
	{
		IList Search(IComparable key);
	}
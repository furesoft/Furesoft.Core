using System;
using System.Collections;

namespace Furesoft.Core.ObjectDB.Btree;

	internal interface IBTreeMultipleValuesPerKey : IBTree
	{
		IList Search(IComparable key);
	}
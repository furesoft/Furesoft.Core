using System;

namespace Furesoft.Core.ObjectDB.Btree
{
	public interface IKeyAndValue
	{
		IComparable GetKey();

		object GetValue();
	}
}
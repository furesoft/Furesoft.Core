using System.Collections.Generic;
using System.Text;

namespace Furesoft.Core.ObjectDB.Tool.Wrappers;

	internal class OdbList<TItem> : List<TItem>, IOdbList<TItem>
	{
		public OdbList()
		{
		}

		public OdbList(int size) : base(size)
		{
		}

		#region IOdbList<E> Members

		public void AddAll(IEnumerable<TItem> collection)
		{
			AddRange(collection);
		}

		public void RemoveAll(IEnumerable<TItem> collection)
		{
			foreach (var item in collection)
				Remove(item);
		}

		#endregion IOdbList<E> Members

		public override string ToString()
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.Append("[");

			foreach (var item in this)
				stringBuilder.Append(item + ", ");

			if (stringBuilder.Length > 3)
				stringBuilder.Remove(stringBuilder.Length - 3, 2);

			stringBuilder.Append("]");

			return stringBuilder.ToString();
		}
	}
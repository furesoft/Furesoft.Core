using System.Collections;

namespace Furesoft.Core.ObjectDB.Btree
{
	internal abstract class IterarorAdapter : IEnumerator
	{
		#region IEnumerator Members

		public object Current
		{
			get { return GetCurrent(); }
		}

		public abstract bool MoveNext();

		public abstract void Reset();

		#endregion IEnumerator Members

		protected abstract object GetCurrent();
	}
}
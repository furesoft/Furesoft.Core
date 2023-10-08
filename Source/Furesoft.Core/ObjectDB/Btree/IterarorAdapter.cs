using System.Collections;

namespace Furesoft.Core.ObjectDB.Btree;

internal abstract class IterarorAdapter : IEnumerator
{
    protected abstract object GetCurrent();

    #region IEnumerator Members

    public object Current => GetCurrent();

    public abstract bool MoveNext();

    public abstract void Reset();

    #endregion IEnumerator Members
}
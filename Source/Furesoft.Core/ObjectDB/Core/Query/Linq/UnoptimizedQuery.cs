using System.Collections;
using Furesoft.Core.ObjectDB.Api.Query;

namespace Furesoft.Core.ObjectDB.Core.Query.Linq;

internal sealed class UnoptimizedQuery<T> : ILinqQueryInternal<T>
{
    public UnoptimizedQuery(IEnumerable<T> result)
    {
        if (result == null)
            throw new ArgumentNullException("result");

        Result = result;
    }

    public IEnumerable<T> Result { get; }

    #region ILinqQueryInternal<T> Members

    public IEnumerator<T> GetEnumerator()
    {
        return Result.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerable<T> UnoptimizedThenBy<TKey>(Func<T, TKey> function)
    {
        return ((IOrderedEnumerable<T>) Result).ThenBy(function);
    }

    public IEnumerable<T> UnoptimizedThenByDescending<TKey>(Func<T, TKey> function)
    {
        return ((IOrderedEnumerable<T>) Result).ThenByDescending(function);
    }

    public IEnumerable<T> UnoptimizedWhere(Func<T, bool> func)
    {
        return Result.Where(func);
    }

    #endregion ILinqQueryInternal<T> Members
}
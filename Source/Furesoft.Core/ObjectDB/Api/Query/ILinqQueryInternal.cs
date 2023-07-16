namespace Furesoft.Core.ObjectDB.Api.Query;

	internal interface ILinqQueryInternal<out T> : ILinqQuery<T>
	{
		IEnumerable<T> UnoptimizedThenBy<TKey>(Func<T, TKey> function);

		IEnumerable<T> UnoptimizedThenByDescending<TKey>(Func<T, TKey> function);

		IEnumerable<T> UnoptimizedWhere(Func<T, bool> func);
	}
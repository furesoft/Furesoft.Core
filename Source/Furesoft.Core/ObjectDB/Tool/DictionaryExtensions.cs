namespace Furesoft.Core.ObjectDB.Tool;

internal static class DictionaryExtensions
{
    internal static TItem GetOrAdd<TKey, TItem>(this Dictionary<TKey, TItem> self, TKey key, Func<TKey, TItem> produce)
    {
        var success = self.TryGetValue(key, out var value);
        if (success)
            return value;

        value = produce(key);
        self.Add(key, value);

        return value;
    }

    internal static TItem GetOrAdd<TKey, TItem>(this Dictionary<TKey, TItem> self, TKey key, TItem item)
    {
        var success = self.TryGetValue(key, out var value);
        if (success)
            return value;

        self.Add(key, item);

        return item;
    }
}
namespace Furesoft.Core.Platforming;

public static class Injector
{
    private static readonly Dictionary<Type, Type> mappings
        = [];

    public static T Get<T>()
    {
        var type = typeof(T);

        return (T) Get(type);
    }

    public static T Get<T>(Type type)
    {
        return (T) Get(type);
    }

    private static object Get(Type type)
    {
        var target = ResolveType(type);
        var constructor = target.GetConstructors()[0];
        var parameters = constructor.GetParameters();

        var resolvedParameters = new List<object>();

        foreach (var item in parameters) resolvedParameters.Add(Get(item.ParameterType));

        return constructor.Invoke(resolvedParameters.ToArray());
    }

    public static void Add<T>(object value)
    {
        mappings.Add(typeof(T), value.GetType());
    }

    private static Type ResolveType(Type type)
    {
        if (mappings.Keys.Contains(type)) return mappings[type];

        return type;
    }

    public static void Add<T, V>() where V : T
    {
        mappings.Add(typeof(T), typeof(V));
    }

    public static void Clear()
    {
        mappings.Clear();
    }
}
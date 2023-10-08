namespace Furesoft.Core.ObjectDB.Container;

internal static class DependencyContainer
{
    private static readonly Dictionary<Type, Func<object>> factory = new();
    private static readonly Dictionary<Type, Func<object, object>> factoryWithArgument = new();

    internal static void Register<TInterface>(Func<object> factoryMethod)
    {
        if (factory.ContainsKey(typeof(TInterface)))
            return;

        factory.Add(typeof(TInterface), factoryMethod);
    }

    internal static void Register<TInterface>(Func<object, object> factoryMethod)
    {
        if (factoryWithArgument.ContainsKey(typeof(TInterface)))
            return;

        factoryWithArgument.Add(typeof(TInterface), factoryMethod);
    }

    internal static TInterface Resolve<TInterface>()
    {
        return (TInterface) factory[typeof(TInterface)]();
    }

    internal static TInterface Resolve<TInterface>(object argument)
    {
        return (TInterface) factoryWithArgument[typeof(TInterface)](argument);
    }
}
namespace Furesoft.Core.Activation;

public class Activator<ActivationStrategy>
    where ActivationStrategy : IActivationStrategy, new()
{
    public static Activator<ActivationStrategy> Instance = new();
    private readonly ActivationStrategy _strategy = new();

    public object CreateInstance(Type type, object[] args)
    {
        return _strategy.Activate(type, args);
    }

    public T CreateInstance<T>(Type type, object[] args)
    {
        return (T) _strategy.Activate(type, args);
    }

    public T CreateInstance<T>(object[] args)
    {
        return (T) CreateInstance(typeof(T), args);
    }

    public T CreateInstance<T>(object arg1)
    {
        return (T) CreateInstance(typeof(T), [arg1]);
    }

    public T CreateInstance<T>(object arg1, object arg2)
    {
        return (T) CreateInstance(typeof(T), [arg1, arg2]);
    }

    public T CreateInstance<T>(object arg1, object arg2, object arg3)
    {
        return (T) CreateInstance(typeof(T), [arg1, arg2, arg3]);
    }
}
namespace Furesoft.Core.Rules.Interfaces;

public interface IDependencyResolver
{
    object GetService(Type serviceType);

    public T GetService<T>()
    {
        return (T) GetService(typeof(T));
    }
}
namespace Furesoft.Core.Rules.Interfaces;

public interface IDependencyResolver
{
    object GetService(Type serviceType);
}
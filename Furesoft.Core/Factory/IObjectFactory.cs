namespace Furesoft.Core.Factory
{
    public interface IObjectFactory
    {
        object Create<T>(params object[] args);
    }
}
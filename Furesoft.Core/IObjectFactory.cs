namespace Furesoft.Core
{
    public interface IObjectFactory
    {
        object Create<T>(params object[] args);
    }
}
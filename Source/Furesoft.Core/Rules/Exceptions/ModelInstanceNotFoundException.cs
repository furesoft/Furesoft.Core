namespace Furesoft.Core.Rules.Exceptions;

public class ModelInstanceNotFoundException : Exception
{
    public ModelInstanceNotFoundException()
    {
    }

    public ModelInstanceNotFoundException(string message) : base(message)
    {
    }

    public ModelInstanceNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
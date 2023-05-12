namespace Furesoft.Core.Rules.Interfaces;

public interface IError
{
    string Message { get; set; }

    Exception Exception { get; set; }
}
using Furesoft.Core.Rules.Interfaces;

namespace Furesoft.Core.Rules.Models;

public class Error : IError
{
    public string Message { get; set; }

    public Exception Exception { get; set; }
}
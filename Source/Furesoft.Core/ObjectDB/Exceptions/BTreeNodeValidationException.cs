namespace Furesoft.Core.ObjectDB.Exceptions;

/// <summary>
///     Exception raised when error in BTrees will appear (validation error)
/// </summary>
public sealed class BTreeNodeValidationException : OdbRuntimeException
{
    internal BTreeNodeValidationException(string message, Exception cause)
        : base(NDatabaseError.BtreeValidationError.AddParameter(message), cause)
    {
    }

    internal BTreeNodeValidationException(string message)
        : base(NDatabaseError.BtreeValidationError.AddParameter(message))
    {
    }
}
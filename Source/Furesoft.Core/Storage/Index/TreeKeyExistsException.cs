namespace Furesoft.Core.Storage.Index;

public class TreeKeyExistsException : Exception
{
    public TreeKeyExistsException(object key) : base("Duplicate key: " + key)
    {
    }
}
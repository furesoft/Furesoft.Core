namespace Furesoft.Core.GraphDb.IO.Blocks;

public abstract class GenericStringBlock(bool used, string data, int id)
{
    public string Data = data;
    public int Id = id;
    public bool Used = used;

    protected GenericStringBlock(GenericStringBlock other) : this(other.Used, other.Data, other.Id)
    {
    }
}
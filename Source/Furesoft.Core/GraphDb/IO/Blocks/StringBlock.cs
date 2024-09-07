namespace Furesoft.Core.GraphDb.IO.Blocks;

public class StringBlock : GenericStringBlock
{
    public StringBlock(GenericStringBlock genericStringBlock) : base(genericStringBlock)
    {
    }

    public StringBlock(bool used, string data, int id) : base(used, data, id)
    {
    }
}
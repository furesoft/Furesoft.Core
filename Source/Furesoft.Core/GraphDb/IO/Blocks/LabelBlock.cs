namespace Furesoft.Core.GraphDb.IO.Blocks;

public class LabelBlock : GenericStringBlock
{
    public LabelBlock(GenericStringBlock genericStringBlock) : base(genericStringBlock)
    {
    }

    public LabelBlock(bool used, string data, int id) : base(used, data, id)
    {
    }
}
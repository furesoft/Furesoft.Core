namespace Furesoft.Core.GraphDb.IO.Blocks;

public class PropertyNameBlock : GenericStringBlock
{
    public PropertyNameBlock(GenericStringBlock genericStringBlock) : base(genericStringBlock)
    {
    }

    public PropertyNameBlock(bool used, string data, int id) : base(used, data, id)
    {
    }
}
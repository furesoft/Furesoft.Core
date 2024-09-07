namespace Furesoft.Core.GraphDb.IO.Blocks;

public class NodePropertyBlock : PropertyBlock
{
    public NodePropertyBlock(PropertyBlock other) : base(other)
    {
    }

    public NodePropertyBlock(int propertyId, bool used, PropertyType propertyType, int propertyNameId, byte[] value,
        int nextPropertyId,
        int nodeId) : base(propertyId, used, propertyType, propertyNameId, value, nextPropertyId, nodeId)
    {
    }
}
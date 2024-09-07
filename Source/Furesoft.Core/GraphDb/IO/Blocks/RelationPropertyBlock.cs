namespace Furesoft.Core.GraphDb.IO.Blocks;

public class RelationPropertyBlock : PropertyBlock
{
    public RelationPropertyBlock(PropertyBlock other) : base(other)
    {
    }

    public RelationPropertyBlock(int propertyId, bool used, PropertyType propertyType, int propertyNameId, byte[] value,
        int nextPropertyId,
        int nodeId) : base(propertyId, used, propertyType, propertyNameId, value, nextPropertyId, nodeId)
    {
    }
}
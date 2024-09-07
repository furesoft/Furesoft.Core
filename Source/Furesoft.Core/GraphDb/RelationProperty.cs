using Furesoft.Core.GraphDb.IO.Blocks;

namespace Furesoft.Core.GraphDb;

public class RelationProperty : Property
{
    public RelationProperty(Relation relation, string key, object value) : base(relation, key, value)
    {
    }

    public RelationProperty(Relation relation, PropertyBlock propertyBlock) : base(relation, propertyBlock)
    {
    }
}
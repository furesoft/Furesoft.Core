namespace Furesoft.Core.GraphDb.IO.Blocks;

public class RelationBlock : IEquatable<RelationBlock>
{
    public int FirstNodeId;
    public int FirstNodeNextRelation;
    public int FirstNodePreviousRelationId;
    public int FirstPropertyId;
    public int LabelId;
    public int RelationId;
    public int SecondNodeId;
    public int SecondNodeNextRelation;
    public int SecondNodePreviousRelationId;

    public bool Used;

    public RelationBlock()
    {
    }

    public RelationBlock(bool used, int firstNodeId, int secondNodeId, int firstNodePreviousRelationId,
        int firstNodeNextRelation, int secondNodePreviousRelationId, int secondNodeNextRelation, int firstPropertyId,
        int labelId, int relationId)
    {
        Used = used;
        FirstNodeId = firstNodeId;
        SecondNodeId = secondNodeId;
        FirstNodePreviousRelationId = firstNodePreviousRelationId;
        FirstNodeNextRelation = firstNodeNextRelation;
        SecondNodePreviousRelationId = secondNodePreviousRelationId;
        SecondNodeNextRelation = secondNodeNextRelation;
        FirstPropertyId = firstPropertyId;
        LabelId = labelId;
        RelationId = relationId;
    }

    public bool Equals(RelationBlock other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return RelationId == other.RelationId;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((RelationBlock)obj);
    }

    public override int GetHashCode()
    {
        return RelationId;
    }

    public static bool operator ==(RelationBlock left, RelationBlock right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(RelationBlock left, RelationBlock right)
    {
        return !Equals(left, right);
    }
}
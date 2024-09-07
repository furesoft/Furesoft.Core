namespace Furesoft.Core.GraphDb.IO.Blocks;

public class NodeBlock(
    bool used,
    int nodeId,
    int firstInRelationId,
    int firstOutRelationId,
    int firstPropertyId,
    int labelId)
    : IEquatable<NodeBlock>
{
    public int FirstInRelationId = firstInRelationId;
    public int FirstOutRelationId = firstOutRelationId;
    public int FirstPropertyId = firstPropertyId;
    public int LabelId = labelId;
    public int NodeId = nodeId;

    public bool Used = used;

    public bool Equals(NodeBlock other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return NodeId == other.NodeId;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((NodeBlock)obj);
    }

    public override int GetHashCode()
    {
        return NodeId;
    }

    public static bool operator ==(NodeBlock left, NodeBlock right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(NodeBlock left, NodeBlock right)
    {
        return !Equals(left, right);
    }
}
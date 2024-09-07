using Furesoft.Core.GraphDb.IO;
using Furesoft.Core.GraphDb.IO.Blocks;

namespace Furesoft.Core.GraphDb;

public class Node : Entity, IEquatable<Node>
{
    private readonly NodeBlock nodeBlock;

    public readonly Dictionary<string, NodeProperty> Properties;
    public List<Relation> InRelations;
    public string Label;

    public int LabelId;
    public int NodeId;

    public List<Relation> OutRelations;

    public Node(string label, DbEngine db, EntityState state)
    {
        NodeId = 0;
        LabelId = 0;
        Label = label;
        Db = db;

        Properties = new Dictionary<string, NodeProperty>();
        OutRelations = [];
        InRelations = [];


        State = state;
        if (state != EntityState.Unchanged)
            Db.ChangedEntities.Add(this);

        nodeBlock = null;
    }


    public Node(NodeBlock nodeBlock, DbEngine db, EntityState state = EntityState.Unchanged)
    {
        NodeId = nodeBlock.NodeId;
        LabelId = nodeBlock.LabelId;

        Label = DbReader.ReadGenericStringBlock(DbControl.LabelPath, LabelId).Data;
        Db = db;

        Properties = new Dictionary<string, NodeProperty>();

        var propertyBlock = DbReader.ReadPropertyBlock(DbControl.NodePropertyPath, nodeBlock.FirstPropertyId);

        while (propertyBlock.PropertyId != 0)
        {
            if (!propertyBlock.Used)
            {
                propertyBlock = DbReader.ReadPropertyBlock(DbControl.NodePropertyPath, propertyBlock.NextPropertyId);
                continue;
            }

            var property = new NodeProperty(this, propertyBlock);
            Properties.Add(property.Key, property);

            propertyBlock = DbReader.ReadPropertyBlock(DbControl.NodePropertyPath, propertyBlock.NextPropertyId);
        }

        OutRelations = null;
        InRelations = null;

        this.nodeBlock = nodeBlock;
    }

    public object this[string key]
    {
        get => Properties[key].Value;
        set
        {
            if (Properties.TryGetValue(key, out var property))
                // modifying existing property:
                property.Value = value;
            else
                // adding new property:
                Properties[key] = new NodeProperty(this, key, value);
        }
    }

    public bool Equals(Node other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return NodeId == other.NodeId;
    }

    public void PullOutRelations()
    {
        OutRelations = [];

        var outRelationBlock = DbReader.ReadRelationBlock(nodeBlock.FirstOutRelationId);

        while (outRelationBlock.RelationId != 0)
        {
            if (!outRelationBlock.Used)
            {
                outRelationBlock = DbReader.ReadRelationBlock(outRelationBlock.FirstNodeNextRelation);
                continue;
            }

            var relation = new Relation(this, null, outRelationBlock);

            OutRelations.Add(relation);

            outRelationBlock = DbReader.ReadRelationBlock(outRelationBlock.FirstNodeNextRelation);
        }
    }

    public void PullInRelations()
    {
        InRelations = [];

        var inRelationBlock = DbReader.ReadRelationBlock(nodeBlock.FirstInRelationId);

        while (inRelationBlock.RelationId != 0)
        {
            if (!inRelationBlock.Used)
            {
                inRelationBlock = DbReader.ReadRelationBlock(inRelationBlock.SecondNodeNextRelation);
                continue;
            }

            var relation = new Relation(null, this, inRelationBlock);

            InRelations.Add(relation);

            inRelationBlock = DbReader.ReadRelationBlock(inRelationBlock.SecondNodeNextRelation);
        }
    }

    public void DeleteProperty(string key)
    {
        Properties.TryGetValue(key, out var property);
        property?.Delete();
        Properties.Remove(key);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Node)obj);
    }

    public override int GetHashCode()
    {
        return NodeId;
    }

    public static bool operator ==(Node left, Node right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Node left, Node right)
    {
        return !Equals(left, right);
    }
}
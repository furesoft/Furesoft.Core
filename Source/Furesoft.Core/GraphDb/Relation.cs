﻿using Furesoft.Core.GraphDb.IO;
using Furesoft.Core.GraphDb.IO.Blocks;
using ZeroFormatter;

namespace Furesoft.Core.GraphDb;

[ZeroFormattable]
public class Relation : Entity, IEquatable<Relation>
{
    public readonly Dictionary<string, RelationProperty> Properties;

    public Node From;
    public string Label;

    public int LabelId;
    public int RelationId;
    public Node To;

    public Relation(Node from, Node to, string label, EntityState state)
    {
        RelationId = 0;

        From = from;
        To = to;

        LabelId = 0;
        Label = label;

        if (from.Db != to.Db) throw new ArgumentException();

        Db = from.Db;

        Properties = new Dictionary<string, RelationProperty>();

        State = state;
        if (state != EntityState.Unchanged)
            Db.ChangedEntities.Add(this);
    }


    public Relation(Node from, Node to, RelationBlock relationBlock)
    {
        RelationId = relationBlock.RelationId;

        LabelId = relationBlock.LabelId;
        Label = DbReader.ReadGenericStringBlock(DbControl.LabelPath, LabelId).Data;

        Properties = new Dictionary<string, RelationProperty>();

        var propertyBlock = DbReader.ReadPropertyBlock(DbControl.RelationPropertyPath, relationBlock.FirstPropertyId);

        while (propertyBlock.PropertyId != 0)
        {
            if (!propertyBlock.Used)
            {
                propertyBlock =
                    DbReader.ReadPropertyBlock(DbControl.RelationPropertyPath, propertyBlock.NextPropertyId);
                continue;
            }

            var property = new RelationProperty(this, propertyBlock);
            Properties.Add(property.Key, property);

            propertyBlock = DbReader.ReadPropertyBlock(DbControl.RelationPropertyPath, propertyBlock.NextPropertyId);
        }


        From = from;
        To = to;

        Db = null;
        if (From != null)
            Db = From.Db;
        else if (To != null) Db = To.Db;

        if (From == null) From = new Node(DbReader.ReadNodeBlock(relationBlock.FirstNodeId), Db);

        if (To == null) To = new Node(DbReader.ReadNodeBlock(relationBlock.SecondNodeId), Db);
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
                Properties[key] = new RelationProperty(this, key, value);
        }
    }

    public bool Equals(Relation other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return RelationId == other.RelationId;
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

        return Equals((Relation)obj);
    }

    public override int GetHashCode()
    {
        return RelationId;
    }

    public static bool operator ==(Relation left, Relation right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Relation left, Relation right)
    {
        return !Equals(left, right);
    }
}
namespace Furesoft.Core.GraphDb;

public class RelationDescription(string label, Dictionary<string, object> props)
{
    public readonly string Label = label;

    public readonly Dictionary<string, object> Props = props;

    public RelationDescription(string label) : this(label, new Dictionary<string, object>())
    {
    }

    public static RelationDescription Any()
    {
        return new RelationDescription(null, new Dictionary<string, object>());
    }

    public bool CheckRelation(Relation relation)
    {
        if (Label != null && Label != relation.Label) return false;

        foreach (var keyValue in Props)
        {
            if (!relation.Properties.ContainsKey(keyValue.Key))
                return false;

            if (!relation[keyValue.Key].Equals(keyValue.Value))
                return false;
        }

        return true;
    }
}
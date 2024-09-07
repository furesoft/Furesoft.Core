namespace Furesoft.Core.GraphDb;

public class NodeDescription(string label, Dictionary<string, object> props)
{
    public readonly string Label = label;
    public readonly Dictionary<string, object> Props = props;

    public NodeDescription(string label) : this(label, new Dictionary<string, object>())
    {
    }

    public static NodeDescription Any()
    {
        return new NodeDescription(null, new Dictionary<string, object>());
    }

    public bool CheckNode(Node node)
    {
        if (Label != null && Label != node.Label) return false;

        foreach (var keyValue in Props)
        {
            if (!node.Properties.ContainsKey(keyValue.Key))
                return false;

            if (!node[keyValue.Key].Equals(keyValue.Value))
                return false;
        }

        return true;
    }
}
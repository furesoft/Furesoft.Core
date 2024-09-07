namespace Furesoft.Core.GraphDb;

public class RelationSet(RelationsDirection direction)
{
    public RelationsDirection Direction = direction;
    public HashSet<Relation> Relations = [];
}
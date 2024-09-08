using System.Linq.Expressions;
using Furesoft.Core.GraphDb.IO;

namespace Furesoft.Core.GraphDb;

public class Query
{
    private readonly List<NodeSet> _nodeSets = [];
    private readonly List<RelationSet> _relationSets = [];
    private readonly DbEngine _db;

    internal Query(DbEngine db)
    {
        _db = db;
    }

    private NodeSet FirstMatch(NodeDescription nodeDescription)
    {
        var nodeBlocks = DbFetcher.SelectNodeBlocksByLabelAndProperties(nodeDescription.Label, nodeDescription.Props);

        var nodeSet = new NodeSet();

        foreach (var nodeBlock in nodeBlocks) nodeSet.Nodes.Add(new Node(nodeBlock, _db));

        return nodeSet;
    }

    private void BackwardCleanup(int relationSetId)
    {
        var relationSet = _relationSets[relationSetId];
        var nextLayerNodes = _nodeSets[relationSetId + 1].Nodes;

        var allRelationsList = relationSet.Relations.ToList();

        foreach (var candidateRelation in allRelationsList)
            if (!nextLayerNodes.Contains(relationSet.Direction == RelationsDirection.Right
                    ? candidateRelation.To
                    : candidateRelation.From))
                relationSet.Relations.Remove(candidateRelation);

        var newPreviousLayerNodes = new HashSet<Node>();

        foreach (var goodRelation in relationSet.Relations)
            newPreviousLayerNodes.Add(relationSet.Direction == RelationsDirection.Right
                ? goodRelation.From
                : goodRelation.To);

        _nodeSets[relationSetId].Nodes = newPreviousLayerNodes;

        if (relationSetId > 0) BackwardCleanup(relationSetId - 1);
    }

    public NodeSet Match<T>(Expression<Func<T, bool>> expression)
    {
        return Match(new NodeDescription(typeof(T).Name, ConvertExprToDictionary(expression)));
    }

    private static Dictionary<string, object> ConvertExprToDictionary<T>(Expression<Func<T, bool>> expression)
    {
        var properties = new Dictionary<string, object>();

        if (expression.Body is BinaryExpression binaryExpression)
        {
            if (binaryExpression.Left is MemberExpression member && binaryExpression.Right is ConstantExpression constant)
            {
                properties.Add(member.Member.Name, constant.Value);
            }
        }

        return properties;
    }

    public RelationSet To<T>(Expression<Func<T, bool>> expression)
    {
        return To(new RelationDescription(typeof(T).Name, ConvertExprToDictionary(expression)));
    }

    public RelationSet To<T>()
    {
        return To(new RelationDescription(typeof(T).Name, []));
    }

    public RelationSet From<T>(Expression<Func<T, bool>> expression)
    {
        return From(new RelationDescription(typeof(T).Name, ConvertExprToDictionary(expression)));
    }

    public RelationSet From<T>()
    {
        return From(new RelationDescription(typeof(T).Name, []));
    }

    public NodeSet Match(NodeDescription nodeDescription)
    {
        if (_nodeSets.Count == 0 && _relationSets.Count == 0)
        {
            var nodeSet = FirstMatch(nodeDescription);
            _nodeSets.Add(nodeSet);

            return nodeSet;
        }

        if (_nodeSets.Count != _relationSets.Count)
            throw new Exception("There cannot be 2 consecutive calls to Match(...) for one Query");

        var lastRelationSet = _relationSets.Last();
        var newLastNodeSet = new NodeSet();

        foreach (var relation in lastRelationSet.Relations)
        {
            var candidateNode = lastRelationSet.Direction == RelationsDirection.Right ? relation.To : relation.From;

            if (nodeDescription.CheckNode(candidateNode)) newLastNodeSet.Nodes.Add(candidateNode);
        }

        _nodeSets.Add(newLastNodeSet);

        return newLastNodeSet;
    }

    public RelationSet To(RelationDescription relationDescription)
    {
        if (_nodeSets.Count != _relationSets.Count + 1) throw new Exception("To/From must be executed after Match");

        var lastNodeLayer = _nodeSets.Last().Nodes;
        var goodRelations = new RelationSet(RelationsDirection.Right);

        foreach (var node in lastNodeLayer)
        {
            node.PullOutRelations();

            foreach (var outRelation in node.OutRelations)
                if (relationDescription.CheckRelation(outRelation))
                    goodRelations.Relations.Add(outRelation);
        }

        _relationSets.Add(goodRelations);

        return goodRelations;
    }

    public RelationSet From(RelationDescription relationDescription)
    {
        if (_nodeSets.Count != _relationSets.Count + 1) throw new Exception("To/From must be executed after Match");

        var lastNodeLayer = _nodeSets.Last().Nodes;
        var goodRelations = new RelationSet(RelationsDirection.Left);

        foreach (var node in lastNodeLayer)
        {
            node.PullInRelations();

            foreach (var inRelation in node.InRelations)
                if (relationDescription.CheckRelation(inRelation))
                    goodRelations.Relations.Add(inRelation);
        }

        _relationSets.Add(goodRelations);

        return goodRelations;
    }

    public void Execute()
    {
        if (_nodeSets.Count != _relationSets.Count + 1)
            throw new Exception("Query cannot end with To/From, please add one more Match");

        if (_relationSets.Count > 0)
            BackwardCleanup(_relationSets.Count - 1);
    }
}
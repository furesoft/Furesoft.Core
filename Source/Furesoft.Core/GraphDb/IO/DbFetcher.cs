﻿using Furesoft.Core.GraphDb.IO.Blocks;

namespace Furesoft.Core.GraphDb.IO;

internal static class DbFetcher
{
    public static List<NodeBlock> SelectAllNodeBlocks()
    {
        var result = new List<NodeBlock>();

        var lastNodeId = DbControl.FetchLastId(DbControl.NodePath);

        for (var nodeId = 1; nodeId < lastNodeId; ++nodeId)
        {
            var candidateNodeBlock = DbReader.ReadNodeBlock(nodeId);
            if (candidateNodeBlock.Used)
                result.Add(candidateNodeBlock);
        }

        return result;
    }

    public static HashSet<NodeBlock> SelectNodeBlocksByLabelAndProperties(string label,
        Dictionary<string, object> props)
    {
        if (label == null && props.Count == 0) return [..SelectAllNodeBlocks()];

        var labelId = 0;

        if (label != null)
        {
            var ok = DbControl.LabelInvertedIndex.TryGetValue(label, out labelId);

            if (!ok) return [];
        }

        if (label != null && props.Count == 0)
        {
            var result = new HashSet<NodeBlock>();

            foreach (var nodeBlock in SelectAllNodeBlocks())
                if (nodeBlock.LabelId == labelId)
                    result.Add(nodeBlock);

            return result;
        }


        var rawProps = new Dictionary<int, object>();
        var fromPropNameIdToGoodNodeBlocks = new Dictionary<int, HashSet<NodeBlock>>();

        foreach (var keyValuePair in props)
        {
            var ok = DbControl.PropertyNameInvertedIndex.TryGetValue(keyValuePair.Key, out var propertyNameId);
            if (!ok) return [];

            fromPropNameIdToGoodNodeBlocks[propertyNameId] = [];
            rawProps[propertyNameId] = keyValuePair.Value;
        }

        var propertyNameIds = new HashSet<int>(fromPropNameIdToGoodNodeBlocks.Keys);
        var lastPropertyId = DbControl.FetchLastId(DbControl.NodePropertyPath);

        for (var propertyId = 1; propertyId < lastPropertyId; ++propertyId)
        {
            var currentPropertyBlock =
                new NodePropertyBlock(DbReader.ReadPropertyBlock(DbControl.NodePropertyPath, propertyId));
            if (!currentPropertyBlock.Used)
                continue;

            if (!propertyNameIds.Contains(currentPropertyBlock.PropertyNameId)) continue;

            switch (currentPropertyBlock.PropertyType)
            {
                case PropertyType.Int:
                    if (BitConverter.ToInt32(currentPropertyBlock.Value, 0) !=
                        (int)rawProps[currentPropertyBlock.PropertyNameId])
                        continue;
                    break;
                case PropertyType.String:
                    if (!DbReader.ReadGenericStringBlock(DbControl.StringPath,
                                BitConverter.ToInt32(currentPropertyBlock.Value, 0)).Data
                            .Equals((string)rawProps[currentPropertyBlock.PropertyNameId]))
                        continue;
                    break;
                case PropertyType.Bool:
                    if (BitConverter.ToBoolean(currentPropertyBlock.Value, 3) !=
                        (bool)rawProps[currentPropertyBlock.PropertyNameId])
                        continue;
                    break;
                case PropertyType.Float:
                    if (BitConverter.ToSingle(currentPropertyBlock.Value, 0) !=
                        (float)rawProps[currentPropertyBlock.PropertyNameId])
                        continue;
                    break;
                default:
                    throw new NotSupportedException();
            }

            var currentNodeBlock = DbReader.ReadNodeBlock(currentPropertyBlock.NodeId);
            if (labelId != 0 && currentNodeBlock.LabelId != labelId) continue;

            fromPropNameIdToGoodNodeBlocks[currentPropertyBlock.PropertyNameId].Add(currentNodeBlock);
        }

        var listOfSets = new List<HashSet<NodeBlock>>(fromPropNameIdToGoodNodeBlocks.Values);

        var outputNodes = listOfSets.Aggregate(
            (h, e) =>
            {
                h.IntersectWith(e);
                return h;
            }
        );

        return outputNodes;
    }
}
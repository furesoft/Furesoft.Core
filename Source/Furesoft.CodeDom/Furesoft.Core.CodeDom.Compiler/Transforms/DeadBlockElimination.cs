using Furesoft.Core.CodeDom.Compiler.Analysis;
using System.Collections.Generic;

namespace Furesoft.Core.CodeDom.Compiler.Transforms;

/// <summary>
/// A transform that removes all blocks not reachable
/// from the entry point block.
/// </summary>
public sealed class DeadBlockElimination : IntraproceduralOptimization
{
    /// <summary>
    /// An instance of the dead block elimination transform.
    /// </summary>
    public static readonly DeadBlockElimination Instance = new();

    private DeadBlockElimination()
    { }

    /// <summary>
    /// Removes dead blocks from a particular graph.
    /// </summary>
    /// <param name="graph">The graph to rewrite.</param>
    /// <returns>A rewritten flow graph.</returns>
    public override FlowGraph Apply(FlowGraph graph)
    {
        var reachability = graph.GetAnalysisResult<BlockReachability>();

        var deadBlocks = new HashSet<BasicBlockTag>(graph.BasicBlockTags);
        deadBlocks.Remove(graph.EntryPointTag);
        deadBlocks.ExceptWith(
            reachability.GetStrictlyReachableBlocks(
                graph.EntryPointTag));

        var graphBuilder = graph.ToBuilder();
        foreach (var tag in deadBlocks)
        {
            graphBuilder.RemoveBasicBlock(tag);
        }
        return graphBuilder.ToImmutable();
    }
}
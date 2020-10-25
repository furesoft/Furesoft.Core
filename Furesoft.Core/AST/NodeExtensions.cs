namespace Furesoft.Core.AST
{
	public static class NodeExtensions
	{
		public static IAstNode WithRange(IAstNode node, NodeRange range)
		{
			node.Range = range;
			return node;
		}
	}
}
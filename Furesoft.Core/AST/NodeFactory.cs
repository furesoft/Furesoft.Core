using System.Collections.Generic;
using Furesoft.Core.AST.Nodes;

namespace Furesoft.Core.AST
{
	public static class NodeFactory
	{
		public static IAstNode Call(string name, NodeRange range, params IAstNode[] args)
		{
			return new CallNode(name, new List<IAstNode>(args), range);
		}

		public static IAstNode Call(string name, params IAstNode[] args)
		{
			return new CallNode(name, new List<IAstNode>(args), default);
		}

		public static IAstNode Id(string name, NodeRange range)
		{
			return new IdentifierNode(name, range);
		}

		public static IAstNode Id(string name)
		{
			return new IdentifierNode(name, default);
		}

		public static IAstNode Literal(object value, NodeRange range)
		{
			return new LiteralNode(value, range);
		}

		public static IAstNode Literal(object value)
		{
			return new LiteralNode(value, default);
		}
	}
}
using System.Collections.Generic;
using Furesoft.Core.AST.Nodes;

namespace Furesoft.Core.AST
{
	public static class NodeFactory
	{
		public static IAstNode Literal(object value)
		{
			return new LiteralNode(value);
		}

		public static IAstNode Id(string name)
		{
			return new IdentifierNode(name);
		}

		public static IAstNode Call(string name, params IAstNode[] args)
		{
			return new CallNode(name, new List<IAstNode>(args));
		}
	}
}
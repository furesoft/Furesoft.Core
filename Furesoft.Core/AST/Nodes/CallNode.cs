using System.Collections.Generic;

namespace Furesoft.Core.AST.Nodes
{
	public struct CallNode : IAstNode
	{
		public CallNode(string name, List<IAstNode> args, NodeRange range)
		{
			Name = name;
			Args = args;
			Range = range;
		}

		public List<IAstNode> Args { get; set; }
		public string Name { get; set; }
		public NodeRange Range { get; set; }

		public override string ToString()
		{
			return Printer.Default.Print(this);
		}

		public void Visit(INodeVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
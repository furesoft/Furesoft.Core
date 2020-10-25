namespace Furesoft.Core.AST.Nodes
{
	public struct IdentifierNode : IAstNode
	{
		public IdentifierNode(string name, NodeRange range)
		{
			Name = name;
			Range = range;
		}

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
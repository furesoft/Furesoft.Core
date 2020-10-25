namespace Furesoft.Core.AST.Nodes
{
	public struct LiteralNode : IAstNode
	{
		public LiteralNode(object value)
		{
			Value = value;
		}

		public object Value { get; set; }

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
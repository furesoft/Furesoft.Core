namespace Furesoft.Core.AST
{
	public interface IAstNode
	{
		NodeRange Range { get; set; }

		string ToString();

		void Visit(INodeVisitor visitor);
	}
}
/// Copyright by Chris Anders (filmee24, Furesoft)
/// Copyright by Chris Anders (filmee24, Furesoft)
namespace Furesoft.Core.AST
{
	public interface IAstNode
	{
		string ToString();

		void Visit(INodeVisitor visitor);
	}
}
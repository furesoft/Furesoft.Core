/// Copyright by Chris Anders (filmee24, Furesoft)
/// Copyright by Chris Anders (filmee24, Furesoft)
using Furesoft.Core.AST.Nodes;

namespace Furesoft.Core.AST
{
	public interface INodeVisitor
	{
		void Visit(LiteralNode lit);

		void Visit(IdentifierNode id);

		void Visit(CallNode call);
	}
}
using Furesoft.Core.AST.Nodes;

namespace Furesoft.Core.AST
{
	public interface IPrinter
	{
		string Print(LiteralNode lit);

		string Print(IdentifierNode id);

		string Print(CallNode call);
	}
}
using Furesoft.Core.AST.Nodes;

namespace Furesoft.Core.AST
{
	public static class IAstNodeHelper
	{
		public static IAstNode Concat(this IAstNode first, IAstNode second)
		{
			var r = new AstNodeList();
			r.Values.Add(first);
			r.Values.Add(second);

			return r;
		}

		public static IAstNode Concat(this AstNodeList c, IAstNode second)
		{
			c.Values.Add(second);
			return c;
		}
	}
}
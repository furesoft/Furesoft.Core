/// Copyright by Chris Anders (filmee24, Furesoft)
/// Copyright by Chris Anders (filmee24, Furesoft)
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Furesoft.Core.AST.Nodes;

namespace Furesoft.Core.AST
{
	public class DefaultPrinter : IPrinter
	{
		public static string SeperateArgs(IEnumerable<IAstNode> a)
		{
			var args = a.Select(_ => _.ToString());
			return string.Join(',', args);
		}

		public string Print(LiteralNode lit)
		{
			return lit.Value.ToString();
		}

		public string Print(IdentifierNode id)
		{
			return id.Name;
		}

		public string Print(CallNode call)
		{
			var sb = new StringBuilder();

			sb.Append(call.Name);
			sb.Append("(");

			sb.Append(SeperateArgs(call.Args));
			sb.Append(")");

			return sb.ToString();
		}
	}
}
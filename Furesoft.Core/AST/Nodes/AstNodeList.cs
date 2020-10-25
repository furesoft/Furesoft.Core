using System.Collections.Generic;
using System.Text;

namespace Furesoft.Core.AST.Nodes
{
	public class AstNodeList : IAstNode
	{
		public List<IAstNode> Values { get; set; } = new List<IAstNode>();

		public override string ToString()
		{
			var sb = new StringBuilder();

			foreach (var v in Values)
			{
				sb.AppendLine(v.ToString());
			}

			return sb.ToString();
		}
	}
}
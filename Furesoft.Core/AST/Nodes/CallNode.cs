using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Furesoft.Core.AST.Nodes
{
	public struct CallNode : IAstNode
	{
		public CallNode(string name, List<IAstNode> args)
		{
			Name = name;
			Args = args;
		}

		public List<IAstNode> Args { get; set; }
		public string Name { get; set; }

		public override string ToString()
		{
			return Printer.Default.Print(this);
		}
	}
}
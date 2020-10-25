using System;
using System.Collections.Generic;

namespace Furesoft.Core.AST.Nodes
{
	public struct IdentifierNode : IAstNode
	{
		public IdentifierNode(string name)
		{
			Name = name;
		}

		public string Name { get; set; }

		public override string ToString()
		{
			return Printer.Default.Print(this);
		}
	}
}
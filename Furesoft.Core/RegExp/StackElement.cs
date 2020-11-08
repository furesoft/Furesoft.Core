using System.Collections.Generic;
using System.Linq;

namespace Furesoft.Core.RegExp
{
	public static class Regex
	{
		public static bool IsMatch(string pattern, string input)
		{
			return false;
		}

		private static void Parse(string re)
		{
			var i = 0;
			var stack = new Stack<Stack<StackItem>>();
			while (i < re.Length)
			{
				var next = re[i];
				/*switch (next)
				{
					case '.':
						Last(stack.Last()).Push(
							new StackItem(StackItemType.Wildcard, Quantifier.ExactlyOne)
							);
						i++;
						break;

					case '?':
						var lastelement = Last(stack.Last());
						break;
				}*/
			}
		}

		private static StackItem Last(Stack<StackItem> stack)
		{
			return stack.Last();
		}
	}

	internal enum Quantifier
	{
		ExactlyOne
	}

	internal enum StackItemType
	{
		Wildcard
	}

	internal class StackItem
	{
		public StackItem(StackItemType type, Quantifier quantifier)
		{
			Type = type;
			Quantifier = quantifier;
		}

		public StackItemType Type { get; set; }
		public Quantifier Quantifier { get; set; }
	}
}
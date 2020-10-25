namespace Furesoft.Core.AST
{

	public struct NodeRange
	{
		public int End => Start + Length;
		public int Length { get; set; }
		public int Start { get; set; }
	}
}
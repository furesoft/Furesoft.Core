namespace Creek.Plugins.Internal.Tools
{
	internal struct LinkItem
	{
		public string Href;
		public string Text;

		public override string ToString()
		{
			return Href + "\n\t" + Text;
		}
	}
}
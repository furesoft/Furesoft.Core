namespace Creek.Extensibility.Addins
{
	using System;

	[AttributeUsage(AttributeTargets.Assembly)]
	public class ManifestAttribute : Attribute
	{
		#region Constructors and Destructors

		public ManifestAttribute(string name)
		{
			Name = name;
		}

		#endregion Constructors and Destructors

		#region Public Properties

		public string Name { get; set; }

		#endregion Public Properties
	}
}
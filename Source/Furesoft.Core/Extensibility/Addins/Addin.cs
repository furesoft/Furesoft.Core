using System;
using System.Collections.Generic;

namespace Furesoft.Core.Extensibility.Addins;

	public class Addin
	{
		#region Fields

		public List<byte[]> Dependencies = new();

		public AppDomain Domain;

		public List<ExtensionNode> ExtensionNodes = new();

		#endregion Fields

		#region Public Properties

		public string Author { get; set; }

		public string Name { get; set; }

		public string Version { get; set; }
		public string Description { get; set; }

		#endregion Public Properties

		#region Properties

		internal string IconPath { get; set; }

		#endregion Properties

		#region Public Methods and Operators

		public void Unload()
		{
			AppDomain.Unload(Domain);
		}

		#endregion Public Methods and Operators
	}
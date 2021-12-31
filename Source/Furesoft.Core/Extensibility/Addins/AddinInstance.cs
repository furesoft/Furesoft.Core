using System.IO;
using System.Reflection;

namespace Furesoft.Core.Extensibility.Addins
{
	public class AddinInstance
	{
		#region Fields

		private readonly object instance;

		#endregion Fields

		#region Constructors and Destructors

		public AddinInstance(object instance)
		{
			this.instance = instance;
		}

		#endregion Constructors and Destructors

		#region Methods

		internal Addin GetAddin()
		{
			var ass = instance.GetType().Assembly;

			var manifest = "manifest.xml";

			var man = ass.GetCustomAttribute<ManifestAttribute>();

			if (man != null)
			{
				manifest = man.Name;
			}

			var str = ass.GetManifestResourceStream(ass.GetName().Name + "." + manifest);

			if (str != null)
			{
				var r = new StreamReader(str).ReadToEnd();
				var ad = new ManifestReader().Read(ass, r);

				return ad;
			}
			return null;
		}

		#endregion Methods
	}
}
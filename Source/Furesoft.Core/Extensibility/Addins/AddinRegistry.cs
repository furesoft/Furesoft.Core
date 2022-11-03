using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;

namespace Furesoft.Core.Extensibility.Addins;

	public class AddinRegistry : Collection<Addin>
	{
		#region Public Properties

		public string Path { get; set; }

		#endregion Public Properties

		#region Public Methods and Operators

		public void Initialize(string path)
		{
			path = path.Replace(
				"[ApplicationData]",
				Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
			path = path.Replace("[StartupPath]", Environment.CurrentDirectory);

			Path = path;

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			if (!Directory.Exists(path + "\\dependencies\\"))
			{
				Directory.CreateDirectory(path + "\\dependencies\\");
			}

			foreach (var f in Directory.GetFiles(path, "*.dll"))
			{
				try
				{
					Assembly ass;
					ass = Assembly.LoadFile(f);

					var addAttr = ass.GetCustomAttributes(typeof(AddinAttribute), false);
					if (addAttr != null && addAttr.Length != 0)
					{
						//domain.Load(ass.FullName);

						var manifest = "manifest.xml";

						var man = ass.GetCustomAttributes(typeof(ManifestAttribute), true);
						if (man.Length == 1)
						{
							if (man[0] != null)
							{
								var m = man[0] as ManifestAttribute;
								manifest = m.Name;
							}
						}

						var str = ass.GetManifestResourceStream(ass.GetName().Name + "." + manifest);

						if (str != null)
						{
							var r = new StreamReader(str).ReadToEnd();
							var ad = new ManifestReader().Read(ass, r);

							var domain = AppDomain.CreateDomain(ad.Name);

							foreach (var dependency in ad.Dependencies)
							{
								domain.Load(dependency);
							}

							ad.Domain = domain;

							Add(ad);
						}
					}
				}
				catch (Exception ex)
				{
				}
			}
		}

		#endregion Public Methods and Operators
	}
namespace Furesoft.Core.Extensibility.Addins
{
	using System.Collections.Generic;
	using System.Linq;

	public class ServiceProviderContainer
	{
		#region Static Fields

		private static readonly Dictionary<string, IServiceProvider> providers =
			new Dictionary<string, IServiceProvider>();

		#endregion Static Fields

		#region Public Methods and Operators

		public static void AddService(IServiceProvider provider)
		{
			if (!providers.ContainsKey(provider.GetType().Name))
			{
				providers.Add(provider.GetType().Name, provider);
			}
		}

		public static T GetService<T>() where T : IServiceProvider
		{
			return (T)providers[typeof(T).Name];
		}

		public static IServiceProvider[] ToArray()
		{
			return providers.Values.ToArray();
		}

		#endregion Public Methods and Operators
	}
}
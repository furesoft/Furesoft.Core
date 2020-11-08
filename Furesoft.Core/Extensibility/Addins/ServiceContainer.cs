namespace Furesoft.Core.Extensibility.Addins
{
	using System;

	public class ServiceContainer
	{
		#region Public Methods and Operators

		public IServiceProvider GetService(Type type)
		{
			return
				Array.Find(ServiceProviderContainer.ToArray(), serviceProvider => serviceProvider.GetType().Name == type.Name);
		}

		public T GetService<T>() where T : IServiceProvider
		{
			return (T)GetService(typeof(T));
		}

		#endregion Public Methods and Operators
	}
}
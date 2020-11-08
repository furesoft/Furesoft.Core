using System;
using System.Collections.Generic;

namespace Furesoft.Core
{
	public interface IDependency
	{
	}

	public static class DependencyContainer
	{
		public static readonly Dictionary<string, Func<IDependency>> Factory =
			new Dictionary<string, Func<IDependency>>();

		public static readonly Dictionary<string, Func<object, IDependency>> FactoryWithArgument =
			new Dictionary<string, Func<object, IDependency>>();

		public static void Register<TInterface>(Func<IDependency> factoryMethod)
			where TInterface : IDependency
		{
			if (Factory.ContainsKey(typeof(TInterface).Name))
				Factory[typeof(TInterface).Name] = factoryMethod;
			else
				Factory.Add(typeof(TInterface).Name, factoryMethod);
		}

		public static void Register<TInterface>(Func<object, IDependency> factoryMethod)
			where TInterface : IDependency
		{
			if (FactoryWithArgument.ContainsKey(typeof(TInterface).Name))
				FactoryWithArgument[typeof(TInterface).Name] = factoryMethod;
			else
				FactoryWithArgument.Add(typeof(TInterface).Name, factoryMethod);
		}

		public static TInterface Resolve<TInterface>()
			where TInterface : IDependency
		{
			return (TInterface)Factory[typeof(TInterface).Name]();
		}

		public static TInterface Resolve<TInterface>(out TInterface o) where TInterface : IDependency
		{
			o = Resolve<TInterface>();
			return o;
		}

		public static TInterface Resolve<TInterface>(params IDependency[] arguments)
			where TInterface : IDependency
		{
			return (TInterface)FactoryWithArgument[typeof(TInterface).Name](arguments);
		}
	}
}
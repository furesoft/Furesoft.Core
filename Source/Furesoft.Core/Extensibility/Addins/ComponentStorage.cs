namespace Furesoft.Core.Extensibility.Addins;

	using System.Collections.Generic;

	public class ComponentStorage
	{
		#region Fields

		private readonly Dictionary<string, object> components = new();

		#endregion Fields

		#region Public Methods and Operators

		public void Add(string name, object com)
		{
			components.Add(name, com);
		}

		public void Add<T>(T com) where T : class, new()
		{
			Add(com.GetType().Name, com);
		}

		public object Get(string name)
		{
			return components[name];
		}

		public T Get<T>() where T : class, new()
		{
			return (T)Get(typeof(T).Name);
		}

		#endregion Public Methods and Operators
	}
using System.Collections.Generic;
using System.Linq;

namespace Creek.Extensibility.Addins
{
	public class ExtensionNode
	{
		#region Fields

		public Dictionary<string, object> Commands { get; set; } = new Dictionary<string, object>();

		public string _path;

		internal List<ExtensionCommand> _nodes = new List<ExtensionCommand>();

		#endregion Fields

		#region Public Methods and Operators

		public T[] CreateInstances<T>() where T : class
		{
			return Commands.Values.ToArray().Select(source => source as T).ToArray();
		}

		public ExtensionCommand GetCommand(string name)
		{
			return _nodes.Find(extensionCommand => extensionCommand.Name == name);
		}

		#endregion Public Methods and Operators
	}
}
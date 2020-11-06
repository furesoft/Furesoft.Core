using System;

namespace Creek.Extensibility.Plugins.Runnable
{
	/// <summary>
	/// This interface is extended by plugins which are used by runnable plugin
	/// applications.
	/// </summary>
	public interface IRunnablePlugIn : IRunnable
	{
		/// <summary>
		/// This event is raised when plugin is started by main application.
		/// </summary>
		event EventHandler Started;

		/// <summary>
		/// This event is raised when plugin is stopped by main application.
		/// </summary>
		event EventHandler Stopped;
	}
}
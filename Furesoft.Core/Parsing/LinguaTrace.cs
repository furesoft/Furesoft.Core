using System.Diagnostics;

namespace Creek.Parsing.Generator
{
	public enum LinguaTraceId
	{
		ID_PARSE = 0x0100,
		ID_PARSE_READTOKEN = 0x0101,
		ID_PARSE_ACTION = 0x0102,

		ID_GENERATE = 0x0200,
		ID_GENERATE_STATE = 0x0201,
		ID_GENERATE_PROCESS_STATE = 0x0202,
		ID_GENERATE_PROCESS_ITEM = 0x0203,
		ID_GENERATE_PROCESS_CONFLICT = 0x0204,
		ID_GENERATE_PROCESS_ACTION = 0x0205,
		ID_GENERATE_PROCESS_TERMINAL = 0x0206
	}

	public static class LinguaTrace
	{
		#region Fields

		private static TraceSource s_traceSource = new TraceSource("Lingua", SourceLevels.All);

		#endregion Fields

		#region Public Properties

		public static TraceSource TraceSource
		{
			get
			{
				return s_traceSource;
			}
		}

		#endregion Public Properties

		#region Public Methods

		internal static void TraceEvent(TraceEventType eventType, int id)
		{
			TraceSource.TraceEvent(eventType, id);
		}

		internal static void TraceEvent(TraceEventType eventType, LinguaTraceId id, string message)
		{
			TraceSource.TraceEvent(eventType, (int)id, message);
		}

		internal static void TraceEvent(TraceEventType eventType, LinguaTraceId id, string format, params object[] args)
		{
			TraceSource.TraceEvent(eventType, (int)id, format, args);
		}

		#endregion Public Methods
	}
}
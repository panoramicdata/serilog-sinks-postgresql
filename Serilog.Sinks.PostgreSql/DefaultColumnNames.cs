namespace Serilog.Sinks.PostgreSql
{
	public static class DefaultColumnNames
	{
		public static readonly string RenderedMesssage = "message";

		public static readonly string MessageTemplate = "message_template";

		public static readonly string Level = "level";

		public static readonly string Timestamp = "timestamp";

		public static readonly string Exception = "exception";

		public static readonly string LogEventSerialized = "log_event";
	}
}
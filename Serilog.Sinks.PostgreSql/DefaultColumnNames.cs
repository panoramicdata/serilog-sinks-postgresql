namespace Serilog.Sinks.PostgreSql
{
	public static class DefaultColumnNames
	{
		public static string RenderedMesssage = "message";

		public static string MessageTemplate = "message_template";

		public static string Level = "level";

		public static string Timestamp = "timestamp";

		public static string Exception = "exception";

		public static string LogEventSerialized = "log_event";
	}
}
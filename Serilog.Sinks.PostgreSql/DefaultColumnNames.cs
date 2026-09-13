namespace Serilog.Sinks.PostgreSql
{
	/// <summary>
	/// The column names used by <see cref="ColumnOptions.Default"/>.
	/// </summary>
	public static class DefaultColumnNames
	{
		/// <summary>
		/// The column holding the rendered message.
		/// </summary>
		public static readonly string RenderedMesssage = "message";

		/// <summary>
		/// The column holding the unrendered message template.
		/// </summary>
		public static readonly string MessageTemplate = "message_template";

		/// <summary>
		/// The column holding the log level.
		/// </summary>
		public static readonly string Level = "level";

		/// <summary>
		/// The column holding the event timestamp.
		/// </summary>
		public static readonly string Timestamp = "timestamp";

		/// <summary>
		/// The column holding the exception, if the event carries one.
		/// </summary>
		public static readonly string Exception = "exception";

		/// <summary>
		/// The column holding the whole event serialised as JSON.
		/// </summary>
		public static readonly string LogEventSerialized = "log_event";
	}
}

using System.Collections.Generic;

namespace Serilog.Sinks.PostgreSql
{
	/// <summary>
	/// Supplies the column layout used when a sink is configured without one.
	/// </summary>
	public static class ColumnOptions
	{
		/// <summary>
		/// A new dictionary of the default column writers, keyed by the names in
		/// <see cref="DefaultColumnNames"/>.
		/// </summary>
		public static IDictionary<string, ColumnWriterBase> Default => new Dictionary<string, ColumnWriterBase>
		  {
				{DefaultColumnNames.RenderedMesssage, new RenderedMessageColumnWriter()},
				{DefaultColumnNames.MessageTemplate, new MessageTemplateColumnWriter()},
				{DefaultColumnNames.Level, new LevelColumnWriter()},
				{DefaultColumnNames.Timestamp, new TimestampColumnWriter()},
				{DefaultColumnNames.Exception, new ExceptionColumnWriter()},
				{DefaultColumnNames.LogEventSerialized, new LogEventSerializedColumnWriter()}
		  };
	}
}

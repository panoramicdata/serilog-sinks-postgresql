using NpgsqlTypes;
using Serilog.Events;
using Serilog.Formatting.Json;
using System;
using System.IO;
using System.Text;

namespace Serilog.Sinks.PostgreSql
{
	/// <summary>
	/// Writes log event as json
	/// </summary>
	public class LogEventSerializedColumnWriter : ColumnWriterBase
	{
		/// <summary>
		/// Initialises a new instance writing to a <c>jsonb</c> column.
		/// </summary>
		public LogEventSerializedColumnWriter() : this(NpgsqlDbType.Jsonb) { }

		/// <summary>
		/// Initialises a new instance writing to a column of the given type.
		/// </summary>
		/// <param name="dbType">The PostgreSQL type of the column being written.</param>
		public LogEventSerializedColumnWriter(NpgsqlDbType dbType) : base(dbType) { }

		/// <inheritdoc />
		public override object GetValue(LogEvent logEvent, IFormatProvider? formatProvider)
			=> LogEventToJson(logEvent, formatProvider);

		private static object LogEventToJson(LogEvent logEvent, IFormatProvider? formatProvider)
		{
			var jsonFormatter = new JsonFormatter(formatProvider: formatProvider);

			var sb = new StringBuilder();
			using (var writer = new StringWriter(sb))
			{
				jsonFormatter.Format(logEvent, writer);
			}

			return sb.ToString();
		}
	}
}

using NpgsqlTypes;
using Serilog.Events;
using Serilog.Formatting.Json;
using System;
using System.IO;
using System.Text;

namespace Serilog.Sinks.PostgreSql
{
	/// <summary>
	/// Writes all event properties as json
	/// </summary>
	public class PropertiesColumnWriter : ColumnWriterBase
	{
		/// <summary>
		/// Initialises a new instance writing to a <c>jsonb</c> column.
		/// </summary>
		public PropertiesColumnWriter() : this(NpgsqlDbType.Jsonb) { }

		/// <summary>
		/// Initialises a new instance writing to a column of the given type.
		/// </summary>
		/// <param name="dbType">The PostgreSQL type of the column being written.</param>
		public PropertiesColumnWriter(NpgsqlDbType dbType) : base(dbType)
		{
		}

		/// <inheritdoc />
		public override object GetValue(LogEvent logEvent, IFormatProvider? formatProvider)
			=> PropertiesToJson(logEvent);

		private static object PropertiesToJson(LogEvent logEvent)
		{
			if (logEvent.Properties.Count == 0)
			{
				return "{}";
			}

			var valuesFormatter = new JsonValueFormatter();

			var sb = new StringBuilder();

			sb.Append('{');

			using (var writer = new StringWriter(sb))
			{
				foreach (var logEventProperty in logEvent.Properties)
				{
					sb.Append('"').Append(logEventProperty.Key).Append("\":");

					valuesFormatter.Format(logEventProperty.Value, writer);

					sb.Append(", ");
				}
			}

			sb.Remove(sb.Length - 2, 2);
			sb.Append('}');

			return sb.ToString();
		}
	}
}

using NpgsqlTypes;
using Serilog.Events;
using Serilog.Formatting.Json;
using System;
using System.Text;

namespace Serilog.Sinks.PostgreSql
{
	/// <summary>
	/// Writes all event properties as json
	/// </summary>
	public class PropertiesColumnWriter : ColumnWriterBase
	{
		public PropertiesColumnWriter(NpgsqlDbType dbType = NpgsqlDbType.Jsonb) : base(dbType)
		{
		}

		public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
			=> PropertiesToJson(logEvent);

		private object PropertiesToJson(LogEvent logEvent)
		{
			if (logEvent.Properties.Count == 0)
				return "{}";

			var valuesFormatter = new JsonValueFormatter();

			var sb = new StringBuilder();

			sb.Append("{");

			using (var writer = new System.IO.StringWriter(sb))
			{
				foreach (var logEventProperty in logEvent.Properties)
				{
					sb.Append('\"').Append(logEventProperty.Key).Append("\":");

					valuesFormatter.Format(logEventProperty.Value, writer);

					sb.Append(", ");
				}
			}

			sb.Remove(sb.Length - 2, 2);
			sb.Append("}");

			return sb.ToString();
		}
	}
}
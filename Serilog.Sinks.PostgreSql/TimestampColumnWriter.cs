using NpgsqlTypes;
using Serilog.Events;
using System;

namespace Serilog.Sinks.PostgreSql
{
	/// <summary>
	/// Writes timestamp part
	/// </summary>
	public class TimestampColumnWriter : ColumnWriterBase
	{

		public TimestampColumnWriter() : this(NpgsqlDbType.Timestamp)
		{
		}

		public TimestampColumnWriter(NpgsqlDbType dbType) : base(dbType)
		{
		}

		public object GetValue(LogEvent logEvent)
			=> GetValue(logEvent, null);

		public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider)
		{
			if (DbType == NpgsqlDbType.Timestamp)
			{
				return logEvent.Timestamp.DateTime;
			}

			return logEvent.Timestamp;
		}
	}
}
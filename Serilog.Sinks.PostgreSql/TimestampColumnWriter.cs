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
		/// <summary>
		/// Initialises a new instance writing to a <c>timestamp</c> column.
		/// </summary>
		public TimestampColumnWriter() : this(NpgsqlDbType.Timestamp)
		{
		}

		/// <summary>
		/// Initialises a new instance writing to a column of the given type.
		/// </summary>
		/// <param name="dbType">The PostgreSQL type of the column being written.</param>
		public TimestampColumnWriter(NpgsqlDbType dbType) : base(dbType)
		{
		}

		/// <inheritdoc />
		public override object GetValue(LogEvent logEvent, IFormatProvider? formatProvider)
		{
			if (DbType == NpgsqlDbType.Timestamp)
			{
				return logEvent.Timestamp.DateTime;
			}

			return logEvent.Timestamp;
		}
	}
}

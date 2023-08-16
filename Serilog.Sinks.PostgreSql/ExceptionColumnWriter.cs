using NpgsqlTypes;
using Serilog.Events;
using System;

namespace Serilog.Sinks.PostgreSql
{
	/// <summary>
	/// Writes exception (just it ToString())
	/// </summary>
	public class ExceptionColumnWriter : ColumnWriterBase
	{
		public ExceptionColumnWriter() : this(NpgsqlDbType.Text) { }

		public ExceptionColumnWriter(NpgsqlDbType dbType = NpgsqlDbType.Text) : base(dbType)
		{
		}

		public object GetValue(LogEvent logEvent)
			=> GetValue(logEvent, null);

		public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider)
			=> logEvent.Exception == null ? (object)DBNull.Value : logEvent.Exception.ToString();
	}
}
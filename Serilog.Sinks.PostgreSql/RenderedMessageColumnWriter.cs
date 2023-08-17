
using NpgsqlTypes;
using Serilog.Events;
using System;

namespace Serilog.Sinks.PostgreSql
{
	/// <summary>
	/// Writes message part
	/// </summary>
	public class RenderedMessageColumnWriter : ColumnWriterBase
	{
		public RenderedMessageColumnWriter() : this(NpgsqlDbType.Text) { }

		public RenderedMessageColumnWriter(NpgsqlDbType dbType) : base(dbType)
		{
		}

		public object GetValue(LogEvent logEvent)
			=> GetValue(logEvent, null);

		public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider)
			=> logEvent.RenderMessage(formatProvider);
	}
}
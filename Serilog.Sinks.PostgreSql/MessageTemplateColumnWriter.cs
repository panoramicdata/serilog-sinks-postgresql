using NpgsqlTypes;
using Serilog.Events;
using System;

namespace Serilog.Sinks.PostgreSql
{
	/// <summary>
	/// Writes non rendered message
	/// </summary>
	public class MessageTemplateColumnWriter : ColumnWriterBase
	{
		public MessageTemplateColumnWriter() : this(NpgsqlDbType.Text) { }

		public MessageTemplateColumnWriter(NpgsqlDbType dbType) : base(dbType)
		{
		}

		public object GetValue(LogEvent logEvent)
			=> GetValue(logEvent, null);

		public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider)
			=> logEvent.MessageTemplate.Text;
	}
}
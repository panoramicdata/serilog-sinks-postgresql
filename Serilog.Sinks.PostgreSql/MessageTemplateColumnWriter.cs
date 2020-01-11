using NpgsqlTypes;
using Serilog.Events;
using System;

namespace Serilog.Sinks.PostgreSQL
{
	/// <summary>
	/// Writes non rendered message
	/// </summary>
	public class MessageTemplateColumnWriter : ColumnWriterBase
	{
		public MessageTemplateColumnWriter(NpgsqlDbType dbType = NpgsqlDbType.Text) : base(dbType)
		{
		}

		public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
			=> logEvent.MessageTemplate.Text;
	}
}
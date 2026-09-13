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
		/// <summary>
		/// Initialises a new instance writing to a <c>text</c> column.
		/// </summary>
		public MessageTemplateColumnWriter() : this(NpgsqlDbType.Text) { }

		/// <summary>
		/// Initialises a new instance writing to a column of the given type.
		/// </summary>
		/// <param name="dbType">The PostgreSQL type of the column being written.</param>
		public MessageTemplateColumnWriter(NpgsqlDbType dbType) : base(dbType)
		{
		}

		/// <inheritdoc />
		public override object GetValue(LogEvent logEvent, IFormatProvider? formatProvider)
			=> logEvent.MessageTemplate.Text;
	}
}

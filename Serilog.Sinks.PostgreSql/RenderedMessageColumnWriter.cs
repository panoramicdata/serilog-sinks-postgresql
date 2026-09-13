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
		/// <summary>
		/// Initialises a new instance writing to a <c>text</c> column.
		/// </summary>
		public RenderedMessageColumnWriter() : this(NpgsqlDbType.Text) { }

		/// <summary>
		/// Initialises a new instance writing to a column of the given type.
		/// </summary>
		/// <param name="dbType">The PostgreSQL type of the column being written.</param>
		public RenderedMessageColumnWriter(NpgsqlDbType dbType) : base(dbType)
		{
		}

		/// <inheritdoc />
		public override object GetValue(LogEvent logEvent, IFormatProvider? formatProvider)
			=> logEvent.RenderMessage(formatProvider);
	}
}

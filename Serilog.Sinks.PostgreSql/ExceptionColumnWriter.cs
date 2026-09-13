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
		/// <summary>
		/// Initialises a new instance writing to a <c>text</c> column.
		/// </summary>
		public ExceptionColumnWriter() : this(NpgsqlDbType.Text) { }

		/// <summary>
		/// Initialises a new instance writing to a column of the given type.
		/// </summary>
		/// <param name="dbType">The PostgreSQL type of the column being written.</param>
		public ExceptionColumnWriter(NpgsqlDbType dbType) : base(dbType)
		{
		}

		/// <inheritdoc />
		public override object GetValue(LogEvent logEvent, IFormatProvider? formatProvider)
			=> logEvent.Exception == null ? DBNull.Value : logEvent.Exception.ToString();
	}
}

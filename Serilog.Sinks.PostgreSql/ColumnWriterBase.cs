using NpgsqlTypes;
using Serilog.Events;
using System;

namespace Serilog.Sinks.PostgreSql
{
	/// <summary>
	/// Base class for writers that project a part of a log event onto a database column.
	/// </summary>
	public abstract class ColumnWriterBase
	{
		/// <summary>
		/// Column type
		/// </summary>
		public NpgsqlDbType DbType { get; }

		/// <summary>
		/// Initialises a new instance of the <see cref="ColumnWriterBase"/> class.
		/// </summary>
		/// <param name="dbType">The PostgreSQL type of the column being written.</param>
		protected ColumnWriterBase(NpgsqlDbType dbType)
		{
			DbType = dbType;
		}

		/// <summary>
		/// Gets the part of the log event to write to the column, using the invariant behaviour of the
		/// writer.
		/// </summary>
		/// <param name="logEvent">The log event to take the value from.</param>
		/// <returns>The value to write, or <see cref="DBNull.Value"/> when there is none.</returns>
		public object GetValue(LogEvent logEvent) => GetValue(logEvent, null);

		/// <summary>
		/// Gets part of log event to write to the column
		/// </summary>
		/// <param name="logEvent">The log event to take the value from.</param>
		/// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
		/// <returns>The value to write, or <see cref="DBNull.Value"/> when there is none.</returns>
		public abstract object GetValue(LogEvent logEvent, IFormatProvider? formatProvider);
	}
}

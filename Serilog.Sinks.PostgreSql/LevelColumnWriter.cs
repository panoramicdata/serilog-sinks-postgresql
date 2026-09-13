using NpgsqlTypes;
using Serilog.Events;
using System;

namespace Serilog.Sinks.PostgreSql
{
	/// <summary>
	/// Writes log level
	/// </summary>
	public class LevelColumnWriter : ColumnWriterBase
	{
		private readonly bool _renderAsText;

		/// <summary>
		/// Initialises a new instance of the <see cref="LevelColumnWriter"/> class.
		/// </summary>
		/// <param name="renderAsText">True to write the level's name, false to write its numeric value.</param>
		/// <param name="dbType">The PostgreSQL type of the column being written.</param>
		public LevelColumnWriter(bool renderAsText = false, NpgsqlDbType dbType = NpgsqlDbType.Integer) : base(dbType)
		{
			_renderAsText = renderAsText;
		}

		/// <inheritdoc />
		public override object GetValue(LogEvent logEvent, IFormatProvider? formatProvider)
		{
			if (_renderAsText)
			{
				return logEvent.Level.ToString();
			}

			return (int)logEvent.Level;
		}
	}
}

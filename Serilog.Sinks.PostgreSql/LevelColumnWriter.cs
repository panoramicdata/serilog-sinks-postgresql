﻿using NpgsqlTypes;
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

		public LevelColumnWriter(bool renderAsText = false, NpgsqlDbType dbType = NpgsqlDbType.Integer) : base(dbType)
		{
			_renderAsText = renderAsText;
		}

		public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
		{
			if (_renderAsText)
			{
				return logEvent.Level.ToString();
			}

			return (int)logEvent.Level;
		}
	}
}
﻿using NpgsqlTypes;
using Serilog.Events;
using System;

namespace Serilog.Sinks.PostgreSQL
{
	/// <summary>
	/// Writes message part
	/// </summary>
	public class RenderedMessageColumnWriter : ColumnWriterBase
	{
		public RenderedMessageColumnWriter(NpgsqlDbType dbType = NpgsqlDbType.Text) : base(dbType)
		{
		}

		public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
			=> logEvent.RenderMessage(formatProvider);
	}
}
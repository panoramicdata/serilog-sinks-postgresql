﻿using NpgsqlTypes;
using Serilog.Events;
using System;

namespace Serilog.Sinks.PostgreSQL
{
	/// <summary>
	/// Writes timestamp part
	/// </summary>
	public class TimestampColumnWriter : ColumnWriterBase
	{
		public TimestampColumnWriter(NpgsqlDbType dbType = NpgsqlDbType.Timestamp) : base(dbType)
		{
		}

		public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
		{
			if (DbType == NpgsqlDbType.Timestamp)
			{
				return logEvent.Timestamp.DateTime;
			}

			return logEvent.Timestamp;
		}
	}
}
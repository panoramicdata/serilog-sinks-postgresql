﻿using System.Collections.Generic;

namespace Serilog.Sinks.PostgreSql
{
	public static class ColumnOptions
	{
		public static IDictionary<string, ColumnWriterBase> Default => new Dictionary<string, ColumnWriterBase>
		  {
				{DefaultColumnNames.RenderedMesssage,new RenderedMessageColumnWriter()},
				{DefaultColumnNames.MessageTemplate, new MessageTemplateColumnWriter()},
				{DefaultColumnNames.Level, new LevelColumnWriter()},
				{DefaultColumnNames.Timestamp, new TimestampColumnWriter()},
				{DefaultColumnNames.Exception, new ExceptionColumnWriter()},
				{DefaultColumnNames.LogEventSerialized, new LogEventSerializedColumnWriter()}
		  };
	}
}
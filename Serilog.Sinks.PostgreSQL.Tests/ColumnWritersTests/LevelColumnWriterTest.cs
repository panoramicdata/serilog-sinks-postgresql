﻿using Serilog.Events;
using Serilog.Parsing;
using System;
using System.Linq;
using Xunit;

namespace Serilog.Sinks.PostgreSQL.Tests
{
	public class LevelColumnWriterTest
	{
		[Fact]
		public void ByDefault_ShouldWriteLevelNo()
		{
			var writer = new LevelColumnWriter();

			var testEvent = new LogEvent(DateTime.Now, LogEventLevel.Debug, null, new MessageTemplate(Enumerable.Empty<MessageTemplateToken>()), Enumerable.Empty<LogEventProperty>());

			var result = writer.GetValue(testEvent);

			Assert.Equal(1, result);
		}

		[Fact]
		public void WriteAsTextIsTrue_ShouldWriteLevelName()
		{
			var writer = new LevelColumnWriter(true);

			var testEvent = new LogEvent(DateTime.Now, LogEventLevel.Debug, null, new MessageTemplate(Enumerable.Empty<MessageTemplateToken>()), Enumerable.Empty<LogEventProperty>());

			var result = writer.GetValue(testEvent);

			Assert.Equal(nameof(LogEventLevel.Debug), result);
		}
	}
}
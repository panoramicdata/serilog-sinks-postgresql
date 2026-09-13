using Serilog.Events;
using Serilog.Parsing;
using System;
using System.Linq;

namespace Serilog.Sinks.PostgreSql.Tests.Support
{
	/// <summary>
	/// Builds the log events the column writer tests exercise, so each test states only the part of
	/// the event it actually cares about.
	/// </summary>
	internal static class TestLogEvent
	{
		public static LogEvent Create(
			LogEventLevel level = LogEventLevel.Debug,
			Exception? exception = null,
			DateTimeOffset? timestamp = null,
			params LogEventProperty[] properties)
			=> new(
				 timestamp ?? DateTimeOffset.Now,
				 level,
				 exception,
				 new MessageTemplate(Enumerable.Empty<MessageTemplateToken>()),
				 properties);

		public static LogEvent WithProperty(string name, object value)
			=> Create(properties: new LogEventProperty(name, new ScalarValue(value)));
	}
}

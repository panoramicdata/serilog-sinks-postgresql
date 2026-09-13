using NpgsqlTypes;
using Serilog.Events;
using Serilog.Sinks.PostgreSql.Tests.Support;
using System;
using Xunit;

namespace Serilog.Sinks.PostgreSql.Tests
{
	/// <summary>
	/// PostgreSQL accepts no NUL character in a text column, and no zero-code unicode escape in a jsonb
	/// one, and fails the whole batch when either arrives. These cover the sink stripping both, since
	/// only the integration tests otherwise reach that code and they need a live database.
	/// </summary>
	public class NullCharacterHandlingTest
	{
		private static object ValueWrittenFor(ColumnWriterBase writer, LogEvent logEvent)
			=> PostgreSQLSink.GetColumnValue(writer, logEvent, null);

		[Fact]
		public void TextValueContainingNullCharacter_ShouldHaveItRemoved()
		{
			var logEvent = TestLogEvent.WithProperty("Prop", "before\0after");

			var result = ValueWrittenFor(new SinglePropertyColumnWriter("Prop", format: "l"), logEvent);

			Assert.Equal("beforeafter", result);
		}

		[Fact]
		public void JsonValueContainingNullEscape_ShouldHaveItRemoved()
		{
			var logEvent = TestLogEvent.WithProperty("Prop", "before\0after");

			var result = (string)ValueWrittenFor(new PropertiesColumnWriter(NpgsqlDbType.Jsonb), logEvent);

			Assert.DoesNotContain("\\u0000", result, StringComparison.OrdinalIgnoreCase);
			Assert.Contains("beforeafter", result);
		}

		[Fact]
		public void JsonValueWithAnEscapedBackslashBeforeU0000_ShouldKeepTheLiteralText()
		{
			// The property holds a backslash followed by the text "u0000", which JSON escapes as
			// "\\u0000". That is not a NUL escape and must survive.
			var logEvent = TestLogEvent.WithProperty("Prop", "\\" + "u0000");

			var result = (string)ValueWrittenFor(new PropertiesColumnWriter(NpgsqlDbType.Jsonb), logEvent);

			Assert.Contains(@"\\u0000", result);
		}

		[Fact]
		public void ValueWithoutNullCharacters_ShouldBeUnchanged()
		{
			var logEvent = TestLogEvent.WithProperty("Prop", "ordinary value");

			var result = ValueWrittenFor(new SinglePropertyColumnWriter("Prop", format: "l"), logEvent);

			Assert.Equal("ordinary value", result);
		}
	}
}

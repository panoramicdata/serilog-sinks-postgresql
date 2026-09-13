using NpgsqlTypes;
using Serilog.Sinks.PostgreSql.Tests.Support;
using System;
using Xunit;

namespace Serilog.Sinks.PostgreSql.Tests
{
	public class TimestampColumnWriterTest
	{
		private static readonly DateTimeOffset TestTimestamp = new(2017, 8, 13, 11, 11, 11, TimeSpan.Zero);

		[Fact]
		public void ByDefault_ShouldReturnTimestampValueWithoutTimezone()
		{
			var writer = new TimestampColumnWriter();

			var result = writer.GetValue(TestLogEvent.Create(timestamp: TestTimestamp));

			Assert.Equal(TestTimestamp.DateTime, result);
		}

		[Fact]
		public void DbTypeWithTimezoneSelected_ShouldReturnTimestampValue()
		{
			var writer = new TimestampColumnWriter(NpgsqlDbType.TimestampTz);

			var result = writer.GetValue(TestLogEvent.Create(timestamp: TestTimestamp));

			Assert.Equal(TestTimestamp, result);
		}
	}
}

using Serilog.Events;
using Serilog.Sinks.PostgreSql.Tests.Support;
using Xunit;

namespace Serilog.Sinks.PostgreSql.Tests
{
	public class LevelColumnWriterTest
	{
		[Fact]
		public void ByDefault_ShouldWriteLevelNo()
		{
			var writer = new LevelColumnWriter();

			var result = writer.GetValue(TestLogEvent.Create(LogEventLevel.Debug));

			Assert.Equal((int)LogEventLevel.Debug, result);
		}

		[Fact]
		public void WriteAsTextIsTrue_ShouldWriteLevelName()
		{
			var writer = new LevelColumnWriter(true);

			var result = writer.GetValue(TestLogEvent.Create(LogEventLevel.Debug));

			Assert.Equal(nameof(LogEventLevel.Debug), result);
		}
	}
}

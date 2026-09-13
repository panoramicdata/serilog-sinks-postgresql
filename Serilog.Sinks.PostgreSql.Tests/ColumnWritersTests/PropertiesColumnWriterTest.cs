using Serilog.Sinks.PostgreSql.Tests.Support;
using Xunit;

namespace Serilog.Sinks.PostgreSql.Tests
{
	public class PropertiesColumnWriterTest
	{
		[Fact]
		public void NoProperties_ShouldReturnEmptyJsonObject()
		{
			var writer = new PropertiesColumnWriter();

			var result = writer.GetValue(TestLogEvent.Create());

			Assert.Equal("{}", result);
		}

		[Fact]
		public void SingleProperty_ShouldReturnJsonObjectWithThatProperty()
		{
			var writer = new PropertiesColumnWriter();

			var result = writer.GetValue(TestLogEvent.WithProperty("TestProperty", "TestValue"));

			Assert.Equal("{\"TestProperty\":\"TestValue\"}", result);
		}
	}
}

using Serilog.Sinks.PostgreSql.Tests.Support;
using System;
using Xunit;

namespace Serilog.Sinks.PostgreSql.Tests
{
	public class ExceptionColumnWriterTest
	{
		[Fact]
		public void ExceptionIsNull_ShouldReturnDbNullValue()
		{
			var writer = new ExceptionColumnWriter();

			var result = writer.GetValue(TestLogEvent.Create());

			Assert.Equal(DBNull.Value, result);
		}

		[Fact]
		public void ExceptionIsPresent_ShouldReturnStringRepresentation()
		{
			var writer = new ExceptionColumnWriter();

			var exception = new InvalidOperationException("Test exception");

			var result = writer.GetValue(TestLogEvent.Create(exception: exception));

			Assert.Equal(exception.ToString(), result);
		}
	}
}

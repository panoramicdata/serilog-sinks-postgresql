using Serilog.Sinks.PostgreSql.Tests.Support;
using System;
using Xunit;

namespace Serilog.Sinks.PostgreSql.Tests
{
	public class SinglePropertyColumnWriterTest
	{
		private const string PropertyName = "TestProperty";

		[Fact]
		public void WithToStringSelected_ShouldRespectFormatPassed()
		{
			const string propertyValue = "TestValue";

			var writer = new SinglePropertyColumnWriter(PropertyName, PropertyWriteMethod.ToString, format: "l");

			var result = writer.GetValue(TestLogEvent.WithProperty(PropertyName, propertyValue));

			Assert.Equal(propertyValue, result);
		}

		[Fact]
		public void PropertyIsNotPresent_ShouldReturnDbNullValue()
		{
			var writer = new SinglePropertyColumnWriter(PropertyName, PropertyWriteMethod.ToString, format: "l");

			var result = writer.GetValue(TestLogEvent.Create());

			Assert.Equal(DBNull.Value, result);
		}

		[Fact]
		public void RawSelectedForScalarProperty_ShouldReturnPropertyValue()
		{
			const int propertyValue = 42;

			var writer = new SinglePropertyColumnWriter(PropertyName, PropertyWriteMethod.Raw);

			var result = writer.GetValue(TestLogEvent.WithProperty(PropertyName, propertyValue));

			Assert.Equal(propertyValue, result);
		}

		[Fact]
		public void JsonSelected_ShouldReturnJsonRepresentation()
		{
			var writer = new SinglePropertyColumnWriter(PropertyName, PropertyWriteMethod.Json);

			var result = writer.GetValue(TestLogEvent.WithProperty(PropertyName, "TestValue"));

			Assert.Equal("\"TestValue\"", result);
		}
	}
}

using System;
using Xunit;

namespace Serilog.Sinks.PostgreSql.Tests
{
	public class SqlIdentifierTest
	{
		[Theory]
		[InlineData("logs")]
		[InlineData("_logs")]
		[InlineData("log_events_2")]
		[InlineData("Logs$")]
		[InlineData("\"Mixed Case\"")]
		[InlineData("\"has \"\"quotes\"\" inside\"")]
		public void ValidIdentifier_ShouldBeReturnedUnchanged(string identifier)
			=> Assert.Equal(identifier, SqlIdentifier.Validate(identifier, "test"));

		[Theory]
		[InlineData("logs; DROP TABLE users")]
		[InlineData("logs)--")]
		[InlineData("2logs")]
		[InlineData("logs name")]
		[InlineData("\"logs\" ; DROP TABLE users --\"")]
		[InlineData("\"\"")]
		[InlineData("")]
		public void InvalidIdentifier_ShouldThrow(string identifier)
			=> Assert.Throws<ArgumentException>(() => SqlIdentifier.Validate(identifier, "test"));

		[Fact]
		public void IdentifierLongerThanPostgresAllows_ShouldThrow()
			=> Assert.Throws<ArgumentException>(() => SqlIdentifier.Validate(new string('a', 64), "test"));

		[Fact]
		public void Quote_UnquotedIdentifier_ShouldQuoteIt()
			=> Assert.Equal("\"Logs\"", SqlIdentifier.Quote("Logs", "test"));

		[Fact]
		public void Quote_IdentifierContainingAQuote_ShouldDoubleIt()
			=> Assert.Equal("\"say \"\"hi\"\"\"", SqlIdentifier.Quote("say \"hi\"", "test"));

		[Fact]
		public void Quote_AlreadyQuotedIdentifier_ShouldBeReturnedUnchanged()
			=> Assert.Equal("\"Logs\"", SqlIdentifier.Quote("\"Logs\"", "test"));

		[Fact]
		public void Unquote_QuotedIdentifier_ShouldReturnTheBareName()
			=> Assert.Equal("Mixed Case", SqlIdentifier.Unquote("\"Mixed Case\""));

		[Fact]
		public void Unquote_UnquotedIdentifier_ShouldReturnItUnchanged()
			=> Assert.Equal("logs", SqlIdentifier.Unquote("logs"));

		[Fact]
		public void QualifiedName_WithoutSchema_ShouldReturnTheTableName()
			=> Assert.Equal("logs", SqlIdentifier.QualifiedName("logs", null));

		[Fact]
		public void QualifiedName_WithSchema_ShouldQualifyTheTableName()
			=> Assert.Equal("audit.logs", SqlIdentifier.QualifiedName("logs", "audit"));

		[Fact]
		public void QualifiedName_WithInjectedSchema_ShouldThrow()
			=> Assert.Throws<ArgumentException>(() => SqlIdentifier.QualifiedName("logs", "public; DROP TABLE users --"));
	}
}

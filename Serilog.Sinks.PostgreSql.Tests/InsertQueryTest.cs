using System.Collections.Generic;
using Xunit;

namespace Serilog.Sinks.PostgreSql.Tests
{
	public class InsertQueryTest
	{
		[Fact]
		public void QuotedColumnNameWithSqlInIt_ShouldNotReachTheValuesList()
		{
			var sink = new PostgreSQLSink(new PostgreSqlSinkOptions
			{
				ConnectionString = "Host=unused",
				TableName = "logs",
				RespectCase = true,
				ColumnOptions = new Dictionary<string, ColumnWriterBase>
				{
					["message"] = new RenderedMessageColumnWriter(),
					["x); DROP TABLE logs; --"] = new LevelColumnWriter(),
				},
			});

			Assert.Equal(
				"INSERT INTO \"logs\" (\"message\", \"x); DROP TABLE logs; --\") VALUES (:p0, :p1)",
				sink.GetInsertQuery());
		}

		[Fact]
		public async System.Threading.Tasks.Task CreateTable_WithSqlInTheTableName_ShouldThrowBeforeRunning()
		{
			await using var connection = new Npgsql.NpgsqlConnection("Host=unused");
			var columns = new Dictionary<string, ColumnWriterBase> { ["message"] = new RenderedMessageColumnWriter() };

			await Assert.ThrowsAsync<System.ArgumentException>(
				() => TableCreator.CreateTableAsync(connection, "t (a int); DROP TABLE users; --", columns));
		}

		[Fact]
		public void Quote_IdentifierWithNul_ShouldThrow()
			=> Assert.Throws<System.ArgumentException>(() => SqlIdentifier.Quote("a\0b", "test"));
	}
}

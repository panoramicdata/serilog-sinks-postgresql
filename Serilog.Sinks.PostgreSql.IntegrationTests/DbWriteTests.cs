using NpgsqlTypes;
using System.Collections.Generic;
using Serilog.Sinks.PostgreSql.IntegrationTests.Objects;
using Xunit;

namespace Serilog.Sinks.PostgreSql.IntegrationTests
{
	public class DbWriteTests : DbWriteTestBase
	{
		[Fact]
		public void Write50Events_ShouldInsert50EventsToDb()
			=> AssertWritesRows(
				 new PostgreSqlSinkOptions
				 {
					 ConnectionString = ConnectionString,
					 TableName = "write_fifty_events",
					 ColumnOptions = SnakeCaseColumns(),
					 NeedAutoCreateTable = true
				 },
				 "write_fifty_events");

		[Fact]
		public void AutoCreateTableIsTrue_ShouldCreateTable()
			=> AssertWritesRows(
				 new PostgreSqlSinkOptions
				 {
					 ConnectionString = ConnectionString,
					 TableName = "logs_auto_created",
					 ColumnOptions = SnakeCaseColumns(includeIntProp: true),
					 NeedAutoCreateTable = true
				 },
				 "logs_auto_created");

		[Fact]
		public void InsertStatements_ShouldInsertEventsToDb()
			=> AssertWritesRows(
				 new PostgreSqlSinkOptions
				 {
					 ConnectionString = ConnectionString,
					 TableName = "insert_statements",
					 ColumnOptions = SnakeCaseColumns(),
					 UseCopy = false,
					 NeedAutoCreateTable = true
				 },
				 "insert_statements");

		[Fact]
		public void PropertyForSinglePropertyColumnWriterDoesNotExist_ShouldStillInsertEvents()
			=> AssertWritesRows(
				 new PostgreSqlSinkOptions
				 {
					 ConnectionString = ConnectionString,
					 TableName = "property_not_exist",
					 ColumnOptions = new Dictionary<string, ColumnWriterBase>()
					 {
						 { "message", new RenderedMessageColumnWriter() },
						 { "absent_prop", new SinglePropertyColumnWriter("NotEnrichedAnywhere", format: "l") }
					 },
					 UseCopy = false,
					 NeedAutoCreateTable = true
				 },
				 "property_not_exist");

		[Fact]
		public void ColumnsAndTableWithDifferentCaseName_ShouldCreateTableAndInsertEvents()
			=> AssertWritesRows(
				 new PostgreSqlSinkOptions
				 {
					 ConnectionString = ConnectionString,
					 TableName = "LogsAutoCreated",
					 ColumnOptions = MixedCaseColumns(),
					 NeedAutoCreateTable = true,
					 RespectCase = true
				 },
				 "\"LogsAutoCreated\"");

		[Fact]
		public void WriteEventWithZeroCodeCharInJson_ShouldInsertEventToDb()
		{
			const string tableName = "write_event_with_zero";

			RequireDatabase();
			DbHelper.RemoveTable(tableName);

			var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test\u0000" };

			var options = new PostgreSqlSinkOptions
			{
				ConnectionString = ConnectionString,
				TableName = tableName,
				ColumnOptions = new Dictionary<string, ColumnWriterBase>()
				{
					{ "message", new RenderedMessageColumnWriter() },
					{ "properties", new LogEventSerializedColumnWriter() },
					{ "props_test", new PropertiesColumnWriter(NpgsqlDbType.Text) }
				},
				NeedAutoCreateTable = true
			};

			using (var logger = new LoggerConfiguration().WriteTo.PostgreSQL(options).CreateLogger())
			{
				logger.Information("Test: {@testObject} testStr: {@testStr:l}", testObject, "stringValue");
			}

			Assert.Equal(1, DbHelper.GetTableRowsCount(tableName));
		}
	}
}

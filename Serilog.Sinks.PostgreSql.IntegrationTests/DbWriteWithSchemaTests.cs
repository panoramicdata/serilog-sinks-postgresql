using Xunit;

namespace Serilog.Sinks.PostgreSql.IntegrationTests
{
	public class DbWriteWithSchemaTests : DbWriteTestBase
	{
		private const string SchemaName = "logs";

		[Fact]
		public void Write50Events_ShouldInsert50EventsToDb()
			=> AssertWritesRows(
				 new PostgreSqlSinkOptions
				 {
					 ConnectionString = ConnectionString,
					 TableName = "logs_with_schema",
					 SchemaName = SchemaName,
					 ColumnOptions = SnakeCaseColumns(),
					 NeedAutoCreateTable = true
				 },
				 $"{SchemaName}.logs_with_schema");

		[Fact]
		public void AutoCreateTableIsTrue_ShouldCreateTable()
			=> AssertWritesRows(
				 new PostgreSqlSinkOptions
				 {
					 ConnectionString = ConnectionString,
					 TableName = "logs_auto_created_w_schema",
					 SchemaName = SchemaName,
					 ColumnOptions = SnakeCaseColumns(includeIntProp: true),
					 NeedAutoCreateTable = true
				 },
				 $"{SchemaName}.logs_auto_created_w_schema");

		[Fact]
		public void ColumnsAndTableWithDifferentCaseName_ShouldCreateTableAndInsertEvents()
			=> AssertWritesRows(
				 new PostgreSqlSinkOptions
				 {
					 ConnectionString = ConnectionString,
					 TableName = "LogsAutoCreatedWithSchema",
					 SchemaName = SchemaName,
					 ColumnOptions = MixedCaseColumns(),
					 NeedAutoCreateTable = true,
					 RespectCase = true
				 },
				 $"\"{SchemaName}\".\"LogsAutoCreatedWithSchema\"");
	}
}

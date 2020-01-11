using NpgsqlTypes;
using Serilog.Sinks.PostgreSql.IntegrationTests.Objects;
using System;
using System.Collections.Generic;
using Xunit;

namespace Serilog.Sinks.PostgreSql.IntegrationTests
{
	public class DbWriteWithSchemaTests
	{
		private const string _connectionString = "Host=localhost;Port=5432;Database=serilog_logs;User ID=serilog;Password=serilog";

		private const string _tableName = "logs_with_schema";
		private const string _schemaName = "logs";

		private readonly string _tableFullName = $"{_schemaName}.{_tableName}";

		private readonly DbHelper _dbHelper = new DbHelper(_connectionString);

		[Fact]
		public void Write50Events_ShouldInsert50EventsToDb()
		{
			_dbHelper.RemoveTable(_tableFullName);

			var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test" };

			var testObj2 = new TestObjectType2 { DateProp1 = DateTime.Now, NestedProp = testObject };

			var columnProps = new Dictionary<string, ColumnWriterBase>
				{
					 {"message", new RenderedMessageColumnWriter() },
					 {"message_template", new MessageTemplateColumnWriter() },
					 {"level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
					 {"raise_date", new TimestampColumnWriter() },
					 {"exception", new ExceptionColumnWriter() },
					 {"properties", new LogEventSerializedColumnWriter() },
					 {"props_test", new PropertiesColumnWriter(NpgsqlDbType.Text) },
					 {"machine_name", new SinglePropertyColumnWriter("MachineName") }
				};

			using (var logger =
				 new LoggerConfiguration().WriteTo.PostgreSQL(
					  _connectionString,
					  _tableName,
					  columnProps,
					  schemaName: _schemaName,
					  needAutoCreateTable: true)
					  .Enrich.WithMachineName()
					  .CreateLogger())
			{
				for (var i = 0; i < 50; i++)
				{
					logger.Information("Test{testNo}: {@testObject} test2: {@testObj2}", i, testObject, testObj2);
				}
			}
			var rowsCount = _dbHelper.GetTableRowsCount(_tableFullName);

			Assert.Equal(50, rowsCount);
		}

		[Fact]
		public void AutoCreateTableIsTrue_ShouldCreateTable()
		{
			const string tableName = "logs_auto_created_w_schema";

			var fullTableName = $"{_schemaName}.{tableName}";
			_dbHelper.RemoveTable(fullTableName);

			var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test" };

			var testObj2 = new TestObjectType2 { DateProp1 = DateTime.Now, NestedProp = testObject };

			var columnProps = new Dictionary<string, ColumnWriterBase>
				{
					 {"message", new RenderedMessageColumnWriter() },
					 {"message_template", new MessageTemplateColumnWriter() },
					 {"level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
					 {"raise_date", new TimestampColumnWriter() },
					 {"exception", new ExceptionColumnWriter() },
					 {"properties", new LogEventSerializedColumnWriter() },
					 {"props_test", new PropertiesColumnWriter(NpgsqlDbType.Text) },
					 {"int_prop_test", new SinglePropertyColumnWriter("testNo", PropertyWriteMethod.Raw, NpgsqlDbType.Integer) },
					 {"machine_name", new SinglePropertyColumnWriter("MachineName", format: "l") }
				};

			var logger =
				 new LoggerConfiguration().WriteTo.PostgreSQL(
					  _connectionString,
					  tableName,
					  columnProps,
					  schemaName: _schemaName,
					  needAutoCreateTable: true)
					  .Enrich.WithMachineName()
					  .CreateLogger();

			const int rowsCount = 50;
			for (var i = 0; i < rowsCount; i++)
			{
				logger.Information("Test{testNo}: {@testObject} test2: {@testObj2} testStr: {@testStr:l}", i, testObject, testObj2, "stringValue");
			}

			logger.Dispose();

			var actualRowsCount = _dbHelper.GetTableRowsCount(fullTableName);

			Assert.Equal(rowsCount, actualRowsCount);
		}

		[Fact]
		public void ColumnsAndTableWithDifferentCaseName_ShouldCreateTableAndIsertEvents()
		{
			const string tableName = "LogsAutoCreatedWithSchema";
			const string schemaName = "Logs";

			var fullTableName = $"\"{schemaName}\".\"{tableName}\"";
			_dbHelper.RemoveTable(fullTableName);

			var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test" };

			var testObj2 = new TestObjectType2 { DateProp1 = DateTime.Now, NestedProp = testObject };

			var columnProps = new Dictionary<string, ColumnWriterBase>
				{
					 {"Message", new RenderedMessageColumnWriter() },
					 {"MessageTemplate", new MessageTemplateColumnWriter() },
					 {"Level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
					 {"RaiseDate", new TimestampColumnWriter() },
					 {"\"Exception\"", new ExceptionColumnWriter() },
					 {"Properties", new LogEventSerializedColumnWriter() },
					 {"PropsTest", new PropertiesColumnWriter(NpgsqlDbType.Text) },
					 {"IntPropTest", new SinglePropertyColumnWriter("testNo", PropertyWriteMethod.Raw, NpgsqlDbType.Integer) },
					 {"MachineName", new SinglePropertyColumnWriter("MachineName", format: "l") }
				};

			const int rowsCount = 50;

			using (var logger =
				 new LoggerConfiguration().WriteTo.PostgreSQL(
					  _connectionString,
					  tableName,
					  columnProps,
					  schemaName: schemaName,
					  needAutoCreateTable: true,
					  respectCase: true)
					  .Enrich.WithMachineName()
					  .CreateLogger())
			{
				for (var i = 0; i < rowsCount; i++)
				{
					logger.Information("Test{testNo}: {@testObject} test2: {@testObj2} testStr: {@testStr:l}", i, testObject, testObj2, "stringValue");
				}
			}

			var actualRowsCount = _dbHelper.GetTableRowsCount(fullTableName);

			Assert.Equal(rowsCount, actualRowsCount);
		}
	}
}
using NpgsqlTypes;
using Serilog.Sinks.PostgreSQL.IntegrationTests.Objects;
using System;
using System.Collections.Generic;
using Xunit;

namespace Serilog.Sinks.PostgreSQL.IntegrationTests
{
	public class DbWriteTests
	{
		private const string _connectionString = "User ID=serilog;Password=serilog;Host=localhost;Port=5432;Database=serilog_logs";

		private readonly DbHelper _dbHelper = new DbHelper(_connectionString);

		[Fact]
		public void Write50Events_ShouldInsert50EventsToDb()
		{
			const string tableName = "write_fifty_events";
			_dbHelper.RemoveTable(tableName);

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
					 {"machine_name", new SinglePropertyColumnWriter("MachineName", format: "l") }
				};

			var logger =
				 new LoggerConfiguration().WriteTo.PostgreSQL(
					  _connectionString,
					  tableName,
					  columnProps,
					  needAutoCreateTable: true)
					  .Enrich.WithMachineName()
					  .CreateLogger();

			const int rowsCount = 50;
			for (var i = 0; i < rowsCount; i++)
			{
				logger.Information("Test{testNo}: {@testObject} test2: {@testObj2} testStr: {@testStr:l}", i, testObject, testObj2, "stringValue");
			}

			logger.Dispose();

			var actualRowsCount = _dbHelper.GetTableRowsCount(tableName);

			Assert.Equal(rowsCount, actualRowsCount);
		}

		[Fact]
		public void WriteEventWithZeroCodeCharInJson_ShouldInsertEventToDb()
		{
			const string tableName = "write_event_with_zero";
			_dbHelper.RemoveTable(tableName);

			var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test\\u0000" };

			var columnProps = new Dictionary<string, ColumnWriterBase>
				{
					 {"message", new RenderedMessageColumnWriter() },
					 {"message_template", new MessageTemplateColumnWriter() },
					 {"level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
					 {"raise_date", new TimestampColumnWriter() },
					 {"exception", new ExceptionColumnWriter() },
					 {"properties", new LogEventSerializedColumnWriter() },
					 {"props_test", new PropertiesColumnWriter(NpgsqlDbType.Text) }
				};

			using (var logger =
				 new LoggerConfiguration().WriteTo.PostgreSQL(
					  _connectionString,
					  tableName,
					  columnProps,
					  needAutoCreateTable: true)
					  .CreateLogger())
			{
				logger.Information("Test: {@testObject} testStr: {@testStr:l}", testObject, "stringValue");
			}

			var actualRowsCount = _dbHelper.GetTableRowsCount(tableName);

			Assert.Equal(1, actualRowsCount);
		}

		[Fact]
		public void QuotedColumnNamesWithInsertStatements_ShouldInsertEventToDb()
		{
			const string tableName = "quoted_column_names";
			_dbHelper.RemoveTable(tableName);

			var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test" };

			var columnProps = new Dictionary<string, ColumnWriterBase>
				{
					 {"message", new RenderedMessageColumnWriter() },
					 {"\"message_template\"", new MessageTemplateColumnWriter() },
					 {"\"level\"", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
					 {"raise_date", new TimestampColumnWriter() },
					 {"exception", new ExceptionColumnWriter() },
					 {"properties", new LogEventSerializedColumnWriter() },
					 {"props_test", new PropertiesColumnWriter(NpgsqlDbType.Text) }
				};

			var logger =
				 new LoggerConfiguration()
					  .WriteTo
					  .PostgreSQL(
							_connectionString,
							tableName,
							columnProps,
							useCopy: false,
							needAutoCreateTable: true)
					  .CreateLogger();

			logger.Information("Test: {@testObject} testStr: {@testStr:l}", testObject, "stringValue");

			logger.Dispose();

			var actualRowsCount = _dbHelper.GetTableRowsCount(tableName);

			Assert.Equal(1, actualRowsCount);
		}

		[Fact]
		public void PropertyForSinglePropertyColumnWriterDoesNotExistsWithInsertStatements_ShouldInsertEventToDb()
		{
			const string tableName = "property_not_exist";
			_dbHelper.RemoveTable(tableName);

			var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test" };

			var columnProps = new Dictionary<string, ColumnWriterBase>
				{
					 {"message", new RenderedMessageColumnWriter() },
					 {"message_template", new MessageTemplateColumnWriter() },
					 {"level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
					 {"raise_date", new TimestampColumnWriter() },
					 {"exception", new ExceptionColumnWriter() },
					 {"properties", new LogEventSerializedColumnWriter() },
					 {"props_test", new PropertiesColumnWriter(NpgsqlDbType.Text) },
					 {"machine_name", new SinglePropertyColumnWriter("MachineName", format: "l") }
				};

			using (var logger =
				 new LoggerConfiguration().WriteTo.PostgreSQL(
					  _connectionString,
					  tableName,
					  columnProps,
					  useCopy: false,
					  needAutoCreateTable: true)
					  .CreateLogger())
			{
				logger.Information("Test: {@testObject} testStr: {@testStr:l}", testObject, "stringValue");
			}

			var actualRowsCount = _dbHelper.GetTableRowsCount(tableName);

			Assert.Equal(1, actualRowsCount);
		}

		[Fact]
		public void AutoCreateTableIsTrue_ShouldCreateTable()
		{
			const string tableName = "logs_auto_created";

			_dbHelper.RemoveTable(tableName);

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

			const int rowsCount = 50;

			using (var logger =
				 new LoggerConfiguration().WriteTo.PostgreSQL(
						_connectionString,
						tableName,
						columnProps,
						needAutoCreateTable: true)
					.Enrich.WithMachineName()
					.CreateLogger())
			{
				for (var i = 0; i < rowsCount; i++)
				{
					logger.Information("Test{testNo}: {@testObject} test2: {@testObj2} testStr: {@testStr:l}", i, testObject, testObj2, "stringValue");
				}
			}

			var actualRowsCount = _dbHelper.GetTableRowsCount(tableName);

			Assert.Equal(rowsCount, actualRowsCount);
		}

		[Fact]
		public void ColumnsAndTableWithDifferentCaseName_ShouldCreateTableAndIsertEvents()
		{
			const string tableName = "LogsAutoCreated";
			var quotedTableName = $"\"{tableName}\"";
			_dbHelper.RemoveTable(quotedTableName);

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
				 new LoggerConfiguration().WriteTo.PostgreSQL(_connectionString, tableName, columnProps, needAutoCreateTable: true, respectCase: true)
					  .Enrich.WithMachineName()
					  .CreateLogger())
			{
				for (var i = 0; i < rowsCount; i++)
				{
					logger.Information("Test{testNo}: {@testObject} test2: {@testObj2} testStr: {@testStr:l}", i, testObject, testObj2, "stringValue");
				}
			}

			var actualRowsCount = _dbHelper.GetTableRowsCount(quotedTableName);

			Assert.Equal(rowsCount, actualRowsCount);
		}
	}
}

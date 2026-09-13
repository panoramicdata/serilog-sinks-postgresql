using NpgsqlTypes;
using Serilog.Sinks.PostgreSql.IntegrationTests.Objects;
using System;
using System.Collections.Generic;
using Xunit;

namespace Serilog.Sinks.PostgreSql.IntegrationTests
{
	/// <summary>
	/// The setup every database write test shares: the connection, the column layout, the test
	/// payload, and the write-then-count cycle each test is really asserting on.
	/// </summary>
	public abstract class DbWriteTestBase
	{
		protected const string ConnectionString =
			"Host=localhost;Port=5432;Database=serilog_logs;User ID=serilog;Password=serilog;Timeout=5";

		protected const int DefaultRowsCount = 50;

		protected DbHelper DbHelper { get; } = new(ConnectionString);

		// Probed once for the whole run: without a database every test would otherwise pay the
		// connection timeout again.
		private static readonly Lazy<bool> DatabaseIsAvailable =
			new(() => new DbHelper(ConnectionString).CanConnect());

		/// <summary>
		/// Skips the calling test when there is no PostgreSQL instance to talk to.
		/// </summary>
		protected static void RequireDatabase()
			=> Assert.SkipUnless(
				 DatabaseIsAvailable.Value,
				 $"No PostgreSQL instance is reachable at '{ConnectionString}'.");

		/// <summary>
		/// The lower-case column layout most of the tests write through.
		/// </summary>
		protected static Dictionary<string, ColumnWriterBase> SnakeCaseColumns(bool includeIntProp = false)
		{
			var columns = new Dictionary<string, ColumnWriterBase>
			{
				{ "message", new RenderedMessageColumnWriter() },
				{ "message_template", new MessageTemplateColumnWriter() },
				{ "level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
				{ "raise_date", new TimestampColumnWriter() },
				{ "exception", new ExceptionColumnWriter() },
				{ "properties", new LogEventSerializedColumnWriter() },
				{ "props_test", new PropertiesColumnWriter(NpgsqlDbType.Text) },
				{ "machine_name", new SinglePropertyColumnWriter("MachineName", format: "l") }
			};

			if (includeIntProp)
			{
				columns.Add("int_prop_test", new SinglePropertyColumnWriter("testNo", PropertyWriteMethod.Raw, NpgsqlDbType.Integer));
			}

			return columns;
		}

		/// <summary>
		/// The mixed-case column layout the identifier-quoting tests write through.
		/// </summary>
		protected static Dictionary<string, ColumnWriterBase> MixedCaseColumns() => new()
		{
			{ "Message", new RenderedMessageColumnWriter() },
			{ "MessageTemplate", new MessageTemplateColumnWriter() },
			{ "Level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
			{ "RaiseDate", new TimestampColumnWriter() },
			{ "Exception", new ExceptionColumnWriter() },
			{ "Properties", new LogEventSerializedColumnWriter() },
			{ "PropsTest", new PropertiesColumnWriter(NpgsqlDbType.Text) },
			{ "IntPropTest", new SinglePropertyColumnWriter("testNo", PropertyWriteMethod.Raw, NpgsqlDbType.Integer) },
			{ "MachineName", new SinglePropertyColumnWriter("MachineName", format: "l") }
		};

		/// <summary>
		/// Drops <paramref name="tableToDrop"/>, writes <paramref name="rowsCount"/> events through a
		/// sink built from <paramref name="options"/>, and asserts that many rows landed in it.
		/// </summary>
		protected void AssertWritesRows(
			PostgreSqlSinkOptions options,
			string tableToDrop,
			int rowsCount = DefaultRowsCount)
		{
			RequireDatabase();

			DbHelper.RemoveTable(tableToDrop);

			var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test" };
			var nestedTestObject = new TestObjectType2 { DateProp1 = DateTime.Now, NestedProp = testObject };

			using (var logger = new LoggerConfiguration()
				 .WriteTo.PostgreSQL(options)
				 .Enrich.WithMachineName()
				 .CreateLogger())
			{
				for (var i = 0; i < rowsCount; i++)
				{
					logger.Information(
						 "Test{testNo}: {@testObject} test2: {@testObj2} testStr: {@testStr:l}",
						 i,
						 testObject,
						 nestedTestObject,
						 "stringValue");
				}
			}

			Assert.Equal(rowsCount, DbHelper.GetTableRowsCount(tableToDrop));
		}
	}
}

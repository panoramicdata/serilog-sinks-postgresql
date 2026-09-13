[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

[![Codacy Badge](https://app.codacy.com/project/badge/grade/serilog-sinks-postgresql)](https://app.codacy.com/gh/panoramicdata/serilog-sinks-postgresql/dashboard)

# Serilog.Sinks.PostgreSql
A [Serilog](https://github.com/serilog/serilog) sink that writes to PostgreSQL

**Package** - [Serilog.Sinks.PostgreSql](https://www.nuget.org/packages/Serilog.Sinks.PostgreSql.PanoramicData/)
| **Platforms** - .NET 10.0

## Code

```csharp
string connectionString = "User ID=serilog;Password=serilog;Host=localhost;Port=5432;Database=logs";

//Used columns (Key is a column name)
//Column type is writer's constructor parameter
IDictionary<string, ColumnWriterBase> columnWriters = new Dictionary<string, ColumnWriterBase>
{
	{"message", new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
	{"message_template", new MessageTemplateColumnWriter(NpgsqlDbType.Text) },
	{"level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
	{"raise_date", new TimestampColumnWriter(NpgsqlDbType.Timestamp) },
	{"exception", new ExceptionColumnWriter(NpgsqlDbType.Text) },
	{"properties", new LogEventSerializedColumnWriter(NpgsqlDbType.Jsonb) },
	{"props_test", new PropertiesColumnWriter(NpgsqlDbType.Jsonb) },
	{"machine_name", new SinglePropertyColumnWriter("MachineName", PropertyWriteMethod.ToString, NpgsqlDbType.Text, "l") }
};

var logger = new LoggerConfiguration()
					.WriteTo.PostgreSQL(connectionString, "logs", columnWriters)
					.CreateLogger();
```

### Configuring the sink

Anything beyond the connection string, table name and columns is set through
`PostgreSqlSinkOptions`:

```csharp
var logger = new LoggerConfiguration()
	.WriteTo.PostgreSQL(new PostgreSqlSinkOptions
	{
		ConnectionString = connectionString,
		TableName = "logs",
		SchemaName = "audit",
		ColumnOptions = columnWriters,
		NeedAutoCreateTable = true,
		RespectCase = true,
		UseCopy = true,
		BatchSizeLimit = 30,
		Period = TimeSpan.FromSeconds(5)
	})
	.CreateLogger();
```

| Option | Default | Purpose |
| --- | --- | --- |
| `ConnectionString` | required | The database to write to. |
| `TableName` | required | The table to write to. |
| `SchemaName` | none | Qualifies the table with a schema. |
| `ColumnOptions` | `ColumnOptions.Default` | The column writers, keyed by column name. |
| `FormatProvider` | none | Culture-specific formatting for rendered values. |
| `Period` | 5 seconds | How often batches are written. |
| `BatchSizeLimit` | 30 | Events per batch. |
| `QueueLimit` | 100,000 | Events held in memory awaiting a write. |
| `UseCopy` | `true` | `COPY` when true, `INSERT` statements when false. |
| `NeedAutoCreateTable` | `false` | Creates the table if it is missing. |
| `RespectCase` | `false` | Quotes identifiers so their casing is preserved. |

### Table auto creation
Set `NeedAutoCreateTable` to `true` and the sink creates the table if it does not exist.
You can change column sizes by setting values in the `TableCreator` class:
```csharp
//Sets size of all BIT and BIT VARYING columns to 20
TableCreator.DefaultBitColumnsLength = 20;

//Sets size of all CHAR columns to 30
TableCreator.DefaultCharColumnsLength = 30;

//Sets size of all VARCHAR columns to 50
TableCreator.DefaultVarcharColumnsLength = 50;
```

### Mixed case table or column names
If your schema, table or column names are in mixed case, set `RespectCase` to `true` and the sink
quotes them so PostgreSQL preserves the casing.

Schema, table and column names are validated as PostgreSQL identifiers before they reach any SQL
text, so a name that is not one is rejected rather than concatenated into a statement.

### Null characters
PostgreSQL stores no NUL character in a `text` column and rejects a zero-code unicode escape in a `jsonb`
one, failing the whole batch either way. The sink strips both before writing, so an event that
picks one up is still recorded rather than silently lost.

## Running the tests

Both test projects are Microsoft.Testing.Platform hosts, so run each one directly:

```sh
dotnet run --project Serilog.Sinks.PostgreSql.Tests
dotnet run --project Serilog.Sinks.PostgreSql.IntegrationTests
```

The integration tests need PostgreSQL on `localhost:5432` with a `serilog` user and a
`serilog_logs` database containing a `logs` schema. They skip themselves when it is unreachable, so
a checkout without one still gives a green run:

```sh
docker run -d --name serilog-pg -p 5432:5432 \
	-e POSTGRES_USER=serilog -e POSTGRES_PASSWORD=serilog -e POSTGRES_DB=serilog_logs \
	postgres:17-alpine
docker exec serilog-pg psql -U serilog -d serilog_logs -c "CREATE SCHEMA IF NOT EXISTS logs;"
```

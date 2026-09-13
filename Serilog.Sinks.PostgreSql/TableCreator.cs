using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serilog.Sinks.PostgreSql
{
	/// <summary>
	/// Creates the log table when the sink is configured to create it.
	/// </summary>
	public static class TableCreator
	{
		/// <summary>
		/// The length given to auto-created <c>character</c> columns.
		/// </summary>
		public static int DefaultCharColumnsLength { get; set; } = 50;

		/// <summary>
		/// The length given to auto-created <c>character varying</c> columns.
		/// </summary>
		public static int DefaultVarcharColumnsLength { get; set; } = 50;

		/// <summary>
		/// The length given to auto-created <c>bit</c> columns.
		/// </summary>
		public static int DefaultBitColumnsLength { get; set; } = 8;

		/// <summary>
		/// Creates the table if it does not already exist.
		/// </summary>
		/// <param name="connection">An open connection to the database.</param>
		/// <param name="tableName">
		/// The validated, optionally schema-qualified name of the table to create.
		/// </param>
		/// <param name="columnsInfo">The validated column names and the writers that fill them.</param>
		/// <returns>A task that completes once the table exists.</returns>
		public static async Task CreateTableAsync(NpgsqlConnection connection, string tableName, IDictionary<string, ColumnWriterBase> columnsInfo)
		{
			await using var command = connection.CreateCommand();

			// The table and column names are interpolated because PostgreSQL does not accept
			// identifiers as query parameters. Both were validated as PostgreSQL identifiers by
			// PostgreSQLSink's constructor, which is the only caller.
			// nosemgrep: csharp.lang.security.sqli.csharp-sqli
			command.CommandText = GetCreateTableQuery(tableName, columnsInfo);

			await command.ExecuteNonQueryAsync().ConfigureAwait(false);
		}

		private static string GetCreateTableQuery(string tableName, IDictionary<string, ColumnWriterBase> columnsInfo)
		{
			var builder = new StringBuilder("CREATE TABLE IF NOT EXISTS ");
			builder.Append(tableName);
			builder.AppendLine(" (");

			builder.AppendLine(string.Join(",\n", columnsInfo.Select(r => $" {r.Key} {GetSqlTypeStr(r.Value.DbType)} ")));

			builder.AppendLine(")");

			return builder.ToString();
		}

		private static string GetSqlTypeStr(NpgsqlDbType dbType) => dbType switch
		{
			NpgsqlDbType.Bigint => "bigint",
			NpgsqlDbType.Double => "double precision",
			NpgsqlDbType.Integer => "integer",
			NpgsqlDbType.Numeric => "numeric",
			NpgsqlDbType.Real => "real",
			NpgsqlDbType.Smallint => "smallint",
			NpgsqlDbType.Boolean => "boolean",
			NpgsqlDbType.Money => "money",
			NpgsqlDbType.Char => $"character({DefaultCharColumnsLength})",
			NpgsqlDbType.Text => "text",
			NpgsqlDbType.Varchar => $"character varying({DefaultVarcharColumnsLength})",
			NpgsqlDbType.Bytea => "bytea",
			NpgsqlDbType.Date => "date",
			NpgsqlDbType.Time => "time",
			NpgsqlDbType.Timestamp => "timestamp",
			NpgsqlDbType.TimestampTz => "timestamp with time zone",
			NpgsqlDbType.Interval => "interval",
			NpgsqlDbType.TimeTz => "time with time zone",
			NpgsqlDbType.Inet => "inet",
			NpgsqlDbType.Cidr => "cidr",
			NpgsqlDbType.MacAddr => "macaddr",
			NpgsqlDbType.Bit => $"bit({DefaultBitColumnsLength})",
			NpgsqlDbType.Varbit => $"bit varying({DefaultBitColumnsLength})",
			NpgsqlDbType.Uuid => "uuid",
			NpgsqlDbType.Xml => "xml",
			NpgsqlDbType.Json => "json",
			NpgsqlDbType.Jsonb => "jsonb",
			_ => throw new ArgumentOutOfRangeException(nameof(dbType), dbType, "Cannot automatically create column of type " + dbType),
		};
	}
}

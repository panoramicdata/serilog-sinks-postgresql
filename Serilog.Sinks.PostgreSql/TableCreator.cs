using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Serilog.Sinks.PostgreSql
{
	public static class TableCreator
	{
		public static int DefaultCharColumnsLength = 50;
		public static int DefaultVarcharColumnsLength = 50;
		public static int DefaultBitColumnsLength = 8;

		public static void CreateTable(NpgsqlConnection connection, string tableName, IDictionary<string, ColumnWriterBase> columnsInfo)
		{
			using var command = connection.CreateCommand();
			command.CommandText = GetCreateTableQuery(tableName, columnsInfo);
			command.ExecuteNonQuery();
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
			_ => throw new ArgumentOutOfRangeException(nameof(dbType), dbType, "Cannot atomatically create column of type " + dbType),
		};
	}
}
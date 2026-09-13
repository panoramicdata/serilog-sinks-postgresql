using Npgsql;
using System;

namespace Serilog.Sinks.PostgreSql.IntegrationTests
{
	/// <summary>
	/// Sets up and inspects the tables the integration tests write to.
	/// </summary>
	public class DbHelper
	{
		private readonly string _connectionString;

		public DbHelper(string connectionString)
		{
			_connectionString = connectionString;
		}

		/// <summary>
		/// True if the test database is reachable. The integration tests skip themselves when it is
		/// not, so a checkout without a PostgreSQL instance still gives a green run.
		/// </summary>
		public bool CanConnect()
		{
			try
			{
				using var connection = new NpgsqlConnection(_connectionString);
				connection.Open();

				return true;
			}
			catch (NpgsqlException)
			{
				return false;
			}
			catch (TimeoutException)
			{
				return false;
			}
		}

		public void RemoveTable(string tableName)
			=> Run("DROP TABLE IF EXISTS ", tableName, command => command.ExecuteNonQuery());

		public void ClearTable(string tableName)
			=> Run("TRUNCATE ", tableName, command => command.ExecuteNonQuery());

		public long GetTableRowsCount(string tableName)
			=> Run("SELECT count(*) FROM ", tableName, command => (long)command.ExecuteScalar()!);

		private T Run<T>(string statement, string tableName, Func<NpgsqlCommand, T> execute)
		{
			// A table name cannot be a query parameter, so it is concatenated into the statement —
			// but only after SqlIdentifier has confirmed it is a valid PostgreSQL identifier, so
			// nothing but an identifier can reach the SQL text.
			var safeTableName = SqlIdentifier.ValidateQualifiedName(tableName, nameof(tableName));

			using var connection = new NpgsqlConnection(_connectionString);
			connection.Open();

			using var command = connection.CreateCommand();

			// nosemgrep: csharp.lang.security.sqli.csharp-sqli
			command.CommandText = statement + safeTableName;

			return execute(command);
		}
	}
}

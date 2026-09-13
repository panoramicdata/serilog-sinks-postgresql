using Npgsql;
using NpgsqlTypes;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serilog.Sinks.PostgreSql
{
	/// <summary>
	/// Writes batches of log events to a PostgreSQL table.
	/// </summary>
	public class PostgreSQLSink : IBatchedLogEventSink
	{
		/// <summary>
		/// The default number of events written in a single batch.
		/// </summary>
		public const int DefaultBatchSizeLimit = PostgreSqlSinkOptions.DefaultBatchSizeLimit;

		/// <summary>
		/// The default maximum number of events held in memory waiting to be written.
		/// </summary>
		public const int DefaultQueueLimit = PostgreSqlSinkOptions.DefaultQueueLimit;

		private readonly string _connectionString;
		private readonly string _fullTableName;
		private readonly IDictionary<string, ColumnWriterBase> _columnOptions;
		private readonly IFormatProvider? _formatProvider;
		private readonly bool _useCopy;

		private bool _isTableCreated;

		/// <summary>
		/// Initialises a new instance of the <see cref="PostgreSQLSink"/> class.
		/// </summary>
		/// <param name="options">The sink configuration.</param>
		/// <exception cref="ArgumentNullException"><paramref name="options"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// The table, schema or a column name is not a valid PostgreSQL identifier.
		/// </exception>
		public PostgreSQLSink(PostgreSqlSinkOptions options)
		{
			ArgumentNullException.ThrowIfNull(options);

			_connectionString = options.ConnectionString;
			_formatProvider = options.FormatProvider;
			_useCopy = options.UseCopy;

			var tableName = options.TableName;
			var schemaName = options.SchemaName;

			if (options.RespectCase)
			{
				tableName = SqlIdentifier.Quote(tableName, nameof(options.TableName));

				if (!string.IsNullOrEmpty(schemaName))
				{
					schemaName = SqlIdentifier.Quote(schemaName!, nameof(options.SchemaName));
				}
			}

			_fullTableName = SqlIdentifier.QualifiedName(tableName, schemaName);

			var columnOptions = options.ColumnOptions ?? ColumnOptions.Default;

			_columnOptions = options.RespectCase
				 ? ValidateColumns(columnOptions, SqlIdentifier.Quote)
				 : ValidateColumns(columnOptions, SqlIdentifier.Validate);

			_isTableCreated = !options.NeedAutoCreateTable;
		}

		/// <summary>
		/// Writes a batch of log events to the table, creating the table first if it was configured to
		/// be created and has not been yet.
		/// </summary>
		/// <param name="batch">The events to write.</param>
		/// <returns>A task that completes once the batch has been written.</returns>
		public async Task EmitBatchAsync(IEnumerable<LogEvent> batch)
		{
			await using var connection = new NpgsqlConnection(_connectionString);
			await connection.OpenAsync().ConfigureAwait(false);

			if (!_isTableCreated)
			{
				await TableCreator.CreateTableAsync(connection, _fullTableName, _columnOptions).ConfigureAwait(false);
				_isTableCreated = true;
			}

			if (_useCopy)
			{
				await ProcessEventsByCopyCommandAsync(batch, connection).ConfigureAwait(false);
			}
			else
			{
				await ProcessEventsByInsertStatementsAsync(batch, connection).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Called when a batching period elapses with no events to write. Does nothing.
		/// </summary>
		/// <returns>A completed task.</returns>
		public Task OnEmptyBatchAsync() => Task.CompletedTask;

		/// <summary>
		/// Reads a value from a column writer, with any NUL characters removed.
		/// </summary>
		/// <remarks>
		/// PostgreSQL stores no NUL character in <c>text</c> or <c>jsonb</c>, and rejects the whole
		/// statement when one arrives. A log event can carry one without anybody intending it —
		/// Serilog turns a literal zero-code escape in a destructured property into a real NUL while
		/// rendering — and losing an entire batch over an unprintable character nobody can see is
		/// worse than writing the event without it.
		/// </remarks>
		internal static object GetColumnValue(ColumnWriterBase writer, LogEvent logEvent, IFormatProvider? formatProvider)
		{
			var value = writer.GetValue(logEvent, formatProvider);

			if (value is not string text)
			{
				return value;
			}

			// A JSON column gets the escape sequence rather than the character, and PostgreSQL rejects
			// that too, so both forms have to go.
			if (writer.DbType is NpgsqlDbType.Json or NpgsqlDbType.Jsonb)
			{
				text = RemoveJsonNullEscapes(text);
			}

			return text.Contains('\0') ? text.Replace("\0", string.Empty) : text;
		}

		/// <summary>
		/// Removes every zero-code unicode escape from a JSON document, leaving other escapes — including
		/// an escaped backslash that merely precedes the literal text <c>u0000</c> — untouched.
		/// </summary>
		private static string RemoveJsonNullEscapes(string json)
		{
			var firstEscape = json.IndexOf('\\');

			if (firstEscape < 0)
			{
				return json;
			}

			var builder = new StringBuilder(json.Length);
			builder.Append(json, 0, firstEscape);

			for (var i = firstEscape; i < json.Length; i++)
			{
				if (json[i] != '\\')
				{
					builder.Append(json[i]);
					continue;
				}

				if (i + 5 < json.Length
					 && (json[i + 1] == 'u' || json[i + 1] == 'U')
					 && json.AsSpan(i + 2, 4).SequenceEqual("0000"))
				{
					i += 5;
					continue;
				}

				// Consume the escape and whatever it escapes as a pair, so an escaped backslash cannot
				// be mistaken for the start of the next escape.
				builder.Append(json[i]);

				if (i + 1 < json.Length)
				{
					builder.Append(json[i + 1]);
					i++;
				}
			}

			return builder.ToString();
		}

		/// <summary>
		/// Returns a copy of <paramref name="columns"/> with every column name put through
		/// <paramref name="prepare"/>, so no unvalidated name can reach the SQL text.
		/// </summary>
		private static Dictionary<string, ColumnWriterBase> ValidateColumns(
			IDictionary<string, ColumnWriterBase> columns,
			Func<string, string, string> prepare)
		{
			var result = new Dictionary<string, ColumnWriterBase>(columns.Count);

			foreach (var column in columns)
			{
				result[prepare(column.Key, "columnOptions")] = column.Value;
			}

			return result;
		}

		private async Task ProcessEventsByInsertStatementsAsync(IEnumerable<LogEvent> events, NpgsqlConnection connection)
		{
			await using var command = connection.CreateCommand();

			// Table and column names cannot be parameterised, so they are interpolated — but every one
			// of them was validated as a PostgreSQL identifier by the constructor. The values are bound
			// as parameters below.
			// nosemgrep: csharp.lang.security.sqli.csharp-sqli
			command.CommandText = GetInsertQuery();

			foreach (var logEvent in events)
			{
				command.Parameters.Clear();

				foreach (var columnOption in _columnOptions)
				{
					command.Parameters.AddWithValue(
						 SqlIdentifier.Unquote(columnOption.Key),
						 columnOption.Value.DbType,
						 GetColumnValue(columnOption.Value, logEvent, _formatProvider));
				}

				await command.ExecuteNonQueryAsync().ConfigureAwait(false);
			}
		}

		private async Task ProcessEventsByCopyCommandAsync(IEnumerable<LogEvent> events, NpgsqlConnection connection)
		{
			// The table and column names in the COPY command were validated as PostgreSQL identifiers
			// by the constructor; the row values go over the binary protocol, not the SQL text.
			// nosemgrep: csharp.lang.security.sqli.csharp-sqli
			await using var binaryCopyWriter = await connection.BeginBinaryImportAsync(GetCopyCommand()).ConfigureAwait(false);
			await WriteToStreamAsync(binaryCopyWriter, events).ConfigureAwait(false);
			await binaryCopyWriter.CompleteAsync().ConfigureAwait(false);
		}

		private string GetCopyCommand()
		{
			var columns = string.Join(", ", _columnOptions.Keys);

			return $"COPY {_fullTableName}({columns}) FROM STDIN BINARY;";
		}

		private string GetInsertQuery()
		{
			var columns = string.Join(", ", _columnOptions.Keys);

			var parameters = string.Join(", ", _columnOptions.Keys.Select(cn => ":" + SqlIdentifier.Unquote(cn)));

			return $"INSERT INTO {_fullTableName} ({columns}) VALUES ({parameters})";
		}

		private async Task WriteToStreamAsync(NpgsqlBinaryImporter writer, IEnumerable<LogEvent> entities)
		{
			foreach (var entity in entities)
			{
				await writer.StartRowAsync().ConfigureAwait(false);

				foreach (var column in _columnOptions)
				{
					await writer.WriteAsync(GetColumnValue(column.Value, entity, _formatProvider), column.Value.DbType).ConfigureAwait(false);
				}
			}
		}
	}
}

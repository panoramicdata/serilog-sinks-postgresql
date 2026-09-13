using System;
using System.Collections.Generic;

namespace Serilog.Sinks.PostgreSql
{
	/// <summary>
	/// Configures a <see cref="PostgreSQLSink"/>.
	/// </summary>
	public sealed class PostgreSqlSinkOptions
	{
		/// <summary>
		/// The default number of events written in a single batch.
		/// </summary>
		public const int DefaultBatchSizeLimit = 30;

		/// <summary>
		/// The default maximum number of events held in memory waiting to be written.
		/// </summary>
		public const int DefaultQueueLimit = 100000;

		/// <summary>
		/// The default time to wait between checking for event batches.
		/// </summary>
		public static readonly TimeSpan DefaultPeriod = TimeSpan.FromSeconds(5);

		/// <summary>
		/// The connection string of the database to write events to.
		/// </summary>
		public required string ConnectionString { get; init; }

		/// <summary>
		/// The name of the table to write events to.
		/// </summary>
		public required string TableName { get; init; }

		/// <summary>
		/// The schema the table lives in, or null to leave the table name unqualified.
		/// </summary>
		public string? SchemaName { get; init; }

		/// <summary>
		/// The column writers keyed by column name, or null to use <see cref="ColumnOptions.Default"/>.
		/// </summary>
		public IDictionary<string, ColumnWriterBase>? ColumnOptions { get; init; }

		/// <summary>
		/// Supplies culture-specific formatting information, or null.
		/// </summary>
		public IFormatProvider? FormatProvider { get; init; }

		/// <summary>
		/// The time to wait between checking for event batches.
		/// </summary>
		public TimeSpan Period { get; init; } = DefaultPeriod;

		/// <summary>
		/// The maximum number of events to include in a single batch.
		/// </summary>
		public int BatchSizeLimit { get; init; } = DefaultBatchSizeLimit;

		/// <summary>
		/// The maximum number of events to hold in memory waiting to be written.
		/// </summary>
		public int QueueLimit { get; init; } = DefaultQueueLimit;

		/// <summary>
		/// True to insert data via the COPY command, false to use INSERT statements.
		/// </summary>
		public bool UseCopy { get; init; } = true;

		/// <summary>
		/// True if the sink should create the table when it does not already exist.
		/// </summary>
		public bool NeedAutoCreateTable { get; init; }

		/// <summary>
		/// True if the sink should quote identifiers so their casing is preserved.
		/// </summary>
		public bool RespectCase { get; init; }
	}
}

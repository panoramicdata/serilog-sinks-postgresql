using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;
using Serilog.Sinks.PostgreSql;
using System;
using System.Collections.Generic;

namespace Serilog
{
	/// <summary>
	/// Adds the PostgreSQL sink to a <see cref="LoggerConfiguration"/>.
	/// </summary>
	public static class LoggerConfigurationPostgreSQLExtensions
	{
		/// <summary>
		/// Default time to wait between checking for event batches.
		/// </summary>
		public static readonly TimeSpan DefaultPeriod = PostgreSqlSinkOptions.DefaultPeriod;

		/// <summary>
		/// Adds a sink which writes to a PostgreSQL table.
		/// </summary>
		/// <param name="sinkConfiguration">The logger configuration.</param>
		/// <param name="options">The sink configuration.</param>
		/// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
		/// <param name="levelSwitch">A switch allowing the pass-through minimum level to be changed at runtime.</param>
		/// <returns>Logger configuration, allowing configuration to continue.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="sinkConfiguration"/> or <paramref name="options"/> is null.</exception>
		public static LoggerConfiguration PostgreSQL(
			this LoggerSinkConfiguration sinkConfiguration,
			PostgreSqlSinkOptions options,
			LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
			LoggingLevelSwitch? levelSwitch = null)
		{
			ArgumentNullException.ThrowIfNull(sinkConfiguration);
			ArgumentNullException.ThrowIfNull(options);

			var batchingSink = new PeriodicBatchingSink(
				 new PostgreSQLSink(options),
				 new PeriodicBatchingSinkOptions
				 {
					 BatchSizeLimit = options.BatchSizeLimit,
					 Period = options.Period,
					 QueueLimit = options.QueueLimit
				 });

			return sinkConfiguration.Sink(batchingSink, restrictedToMinimumLevel, levelSwitch);
		}

		/// <summary>
		/// Adds a sink which writes to a PostgreSQL table, using the default batching behaviour.
		/// </summary>
		/// <param name="sinkConfiguration">The logger configuration.</param>
		/// <param name="connectionString">The connection string to the database where to store the events.</param>
		/// <param name="tableName">Name of the table to store the events in.</param>
		/// <param name="columnOptions">Table column writers, or null to use <see cref="ColumnOptions.Default"/>.</param>
		/// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
		/// <param name="levelSwitch">A switch allowing the pass-through minimum level to be changed at runtime.</param>
		/// <returns>Logger configuration, allowing configuration to continue.</returns>
		/// <remarks>
		/// Use the <see cref="PostgreSqlSinkOptions"/> overload to set the schema, batching, automatic
		/// table creation or identifier casing.
		/// </remarks>
		public static LoggerConfiguration PostgreSQL(
			this LoggerSinkConfiguration sinkConfiguration,
			string connectionString,
			string tableName,
			IDictionary<string, ColumnWriterBase>? columnOptions = null,
			LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
			LoggingLevelSwitch? levelSwitch = null)
			=> sinkConfiguration.PostgreSQL(
				 new PostgreSqlSinkOptions
				 {
					 ConnectionString = connectionString,
					 TableName = tableName,
					 ColumnOptions = columnOptions
				 },
				 restrictedToMinimumLevel,
				 levelSwitch);
	}
}

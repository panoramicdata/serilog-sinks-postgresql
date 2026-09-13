using NpgsqlTypes;
using Serilog.Events;
using Serilog.Formatting.Json;
using System;
using System.IO;
using System.Text;

namespace Serilog.Sinks.PostgreSql
{
	/// <summary>
	/// Write single event property
	/// </summary>
	public class SinglePropertyColumnWriter : ColumnWriterBase
	{
		/// <summary>
		/// The name of the log event property this writer takes its value from.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// How the property is rendered.
		/// </summary>
		public PropertyWriteMethod WriteMethod { get; }

		/// <summary>
		/// The format passed to the property when <see cref="WriteMethod"/> is
		/// <see cref="PropertyWriteMethod.ToString"/>, or null for the default.
		/// </summary>
		public string? Format { get; }

		/// <summary>
		/// Initialises a new instance of the <see cref="SinglePropertyColumnWriter"/> class.
		/// </summary>
		/// <param name="propertyName">The name of the log event property to write.</param>
		/// <param name="writeMethod">How the property is rendered.</param>
		/// <param name="dbType">The PostgreSQL type of the column being written.</param>
		/// <param name="format">The format to render the property with, or null for the default.</param>
		public SinglePropertyColumnWriter(string propertyName, PropertyWriteMethod writeMethod = PropertyWriteMethod.ToString,
														NpgsqlDbType dbType = NpgsqlDbType.Text, string? format = null) : base(dbType)
		{
			Name = propertyName;
			WriteMethod = writeMethod;
			Format = format;
		}

		/// <inheritdoc />
		public override object GetValue(LogEvent logEvent, IFormatProvider? formatProvider)
		{
			if (!logEvent.Properties.TryGetValue(Name, out var property))
			{
				return DBNull.Value;
			}

			switch (WriteMethod)
			{
				case PropertyWriteMethod.Raw:
					return GetPropertyValue(property);

				case PropertyWriteMethod.Json:
					var valuesFormatter = new JsonValueFormatter();

					var sb = new StringBuilder();

					using (var writer = new StringWriter(sb))
					{
						valuesFormatter.Format(property, writer);
					}

					return sb.ToString();

				default:
					return property.ToString(Format, formatProvider);
			}
		}

		private static object GetPropertyValue(LogEventPropertyValue logEventProperty)
		{
			//TODO: Add support for arrays
			if (logEventProperty is ScalarValue scalarValue)
			{
				return scalarValue.Value ?? DBNull.Value;
			}

			return logEventProperty;
		}
	}
}

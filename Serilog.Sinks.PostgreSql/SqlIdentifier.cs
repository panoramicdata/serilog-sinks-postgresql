using System;
using System.Collections.Generic;
using System.Text;

namespace Serilog.Sinks.PostgreSql
{
	/// <summary>
	/// Validates and quotes PostgreSQL identifiers.
	/// </summary>
	/// <remarks>
	/// Table, schema and column names cannot be supplied as query parameters, so they have to be
	/// interpolated into the SQL text. Everything that reaches the SQL text therefore passes through
	/// here first: an identifier is either a plain identifier matching PostgreSQL's unquoted rules,
	/// or it is a double-quoted identifier whose embedded quotes are doubled. Anything else throws.
	/// </remarks>
	internal static class SqlIdentifier
	{
		/// <summary>
		/// The maximum length PostgreSQL allows for an identifier (NAMEDATALEN - 1).
		/// </summary>
		private const int MaxLength = 63;

		/// <summary>
		/// Returns <paramref name="identifier"/> in a form that is safe to interpolate into SQL text.
		/// </summary>
		/// <param name="identifier">The identifier to validate. May already be double-quoted.</param>
		/// <param name="parameterName">The name of the caller's argument, used in exception messages.</param>
		/// <returns>The identifier, quoted if it was already quoted, otherwise unchanged.</returns>
		/// <exception cref="ArgumentException">The identifier is not a valid PostgreSQL identifier.</exception>
		public static string Validate(string identifier, string parameterName)
		{
			if (string.IsNullOrEmpty(identifier))
			{
				throw new ArgumentException("An identifier cannot be null or empty.", parameterName);
			}

			return IsQuoted(identifier)
				 ? ValidateQuoted(identifier, parameterName)
				 : ValidatePlain(identifier, parameterName);
		}

		/// <summary>
		/// Returns <paramref name="identifier"/> as a double-quoted identifier, escaping any embedded
		/// quotes. An identifier that is already quoted is validated and returned unchanged.
		/// </summary>
		/// <param name="identifier">The identifier to quote.</param>
		/// <param name="parameterName">The name of the caller's argument, used in exception messages.</param>
		/// <returns>The double-quoted identifier.</returns>
		/// <exception cref="ArgumentException">The identifier is not a valid PostgreSQL identifier.</exception>
		public static string Quote(string identifier, string parameterName)
		{
			if (string.IsNullOrEmpty(identifier))
			{
				throw new ArgumentException("An identifier cannot be null or empty.", parameterName);
			}

			if (IsQuoted(identifier))
			{
				return ValidateQuoted(identifier, parameterName);
			}

			if (identifier.Length > MaxLength)
			{
				throw new ArgumentException($"Identifier '{identifier}' exceeds the maximum length of {MaxLength} characters.", parameterName);
			}

			return "\"" + identifier.Replace("\"", "\"\"") + "\"";
		}

		/// <summary>
		/// Returns a schema-qualified name built from an optional schema and a table name.
		/// </summary>
		/// <param name="tableName">The table name.</param>
		/// <param name="schemaName">The schema name, or null/empty for no schema qualification.</param>
		/// <returns>The validated, optionally schema-qualified table name.</returns>
		public static string QualifiedName(string tableName, string? schemaName)
		{
			var table = Validate(tableName, nameof(tableName));

			return string.IsNullOrEmpty(schemaName)
				 ? table
				 : Validate(schemaName!, nameof(schemaName)) + "." + table;
		}

		/// <summary>
		/// Validates a name that may already be schema-qualified, such as <c>audit.logs</c> or
		/// <c>"Audit"."Logs"</c>.
		/// </summary>
		/// <param name="qualifiedName">The name to validate.</param>
		/// <param name="parameterName">The name of the caller's argument, used in exception messages.</param>
		/// <returns>The validated name, unchanged.</returns>
		/// <exception cref="ArgumentException">Any part of the name is not a valid identifier.</exception>
		public static string ValidateQualifiedName(string qualifiedName, string parameterName)
		{
			if (string.IsNullOrEmpty(qualifiedName))
			{
				throw new ArgumentException("A qualified name cannot be null or empty.", parameterName);
			}

			var parts = SplitOnUnquotedDots(qualifiedName, parameterName);

			if (parts.Count > 2)
			{
				throw new ArgumentException($"'{qualifiedName}' has more than the schema and table parts a qualified name may have.", parameterName);
			}

			foreach (var part in parts)
			{
				Validate(part, parameterName);
			}

			return qualifiedName;
		}

		private static List<string> SplitOnUnquotedDots(string value, string parameterName)
		{
			var parts = new List<string>();
			var current = new StringBuilder();
			var inQuotes = false;

			for (var i = 0; i < value.Length; i++)
			{
				var character = value[i];

				if (character == '"')
				{
					// A doubled quote inside a quoted identifier is an escaped quote, not the end of
					// it, so it must not flip the quoting state.
					if (inQuotes && i + 1 < value.Length && value[i + 1] == '"')
					{
						current.Append("\"\"");
						i++;
						continue;
					}

					inQuotes = !inQuotes;
				}

				if (character == '.' && !inQuotes)
				{
					parts.Add(current.ToString());
					current.Clear();
					continue;
				}

				current.Append(character);
			}

			if (inQuotes)
			{
				throw new ArgumentException($"'{value}' has an unterminated quoted identifier.", parameterName);
			}

			parts.Add(current.ToString());

			return parts;
		}

		/// <summary>
		/// Removes the surrounding double quotes from an identifier, if it has any, undoubling any
		/// escaped quotes inside it.
		/// </summary>
		/// <param name="identifier">The identifier to unquote.</param>
		/// <returns>The bare identifier text.</returns>
		public static string Unquote(string identifier)
			=> IsQuoted(identifier)
				 ? identifier.Substring(1, identifier.Length - 2).Replace("\"\"", "\"")
				 : identifier;

		private static bool IsQuoted(string identifier)
			=> identifier.Length >= 2 && identifier[0] == '"' && identifier[identifier.Length - 1] == '"';

		private static string ValidateQuoted(string identifier, string parameterName)
		{
			var inner = identifier.Substring(1, identifier.Length - 2);

			// Every double quote inside a quoted identifier must be doubled. Scanning for runs of
			// quotes catches an identifier that closes early and reopens, which would otherwise let
			// arbitrary SQL out of the quotes.
			for (var i = 0; i < inner.Length; i++)
			{
				if (inner[i] != '"')
				{
					continue;
				}

				if (i + 1 >= inner.Length || inner[i + 1] != '"')
				{
					throw new ArgumentException($"Quoted identifier {identifier} contains an unescaped double quote.", parameterName);
				}

				i++;
			}

			if (inner.Replace("\"\"", "\"").Length is 0 or > MaxLength)
			{
				throw new ArgumentException($"Quoted identifier {identifier} must be between 1 and {MaxLength} characters long.", parameterName);
			}

			return identifier;
		}

		private static string ValidatePlain(string identifier, string parameterName)
		{
			if (identifier.Length > MaxLength)
			{
				throw new ArgumentException($"Identifier '{identifier}' exceeds the maximum length of {MaxLength} characters.", parameterName);
			}

			if (!IsPlainIdentifierStart(identifier[0]))
			{
				throw new ArgumentException($"Identifier '{identifier}' must start with a letter or underscore, or be double-quoted.", parameterName);
			}

			foreach (var character in identifier)
			{
				if (!IsPlainIdentifierPart(character))
				{
					throw new ArgumentException($"Identifier '{identifier}' contains the invalid character '{character}'. Double-quote the identifier if it needs one.", parameterName);
				}
			}

			return identifier;
		}

		private static bool IsPlainIdentifierStart(char character)
			=> char.IsLetter(character) || character == '_';

		private static bool IsPlainIdentifierPart(char character)
			=> char.IsLetterOrDigit(character) || character == '_' || character == '$';
	}
}

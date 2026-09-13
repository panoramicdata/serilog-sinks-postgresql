namespace Serilog.Sinks.PostgreSql
{
	/// <summary>
	/// How a <see cref="SinglePropertyColumnWriter"/> renders the property it writes.
	/// </summary>
	public enum PropertyWriteMethod
	{
		/// <summary>
		/// Writes the underlying value of a scalar property unchanged.
		/// </summary>
		Raw = 0,

		/// <summary>
		/// Writes the property rendered as text, honouring the writer's format.
		/// </summary>
		ToString = 1,

		/// <summary>
		/// Writes the property serialised as JSON.
		/// </summary>
		Json = 2
	}
}

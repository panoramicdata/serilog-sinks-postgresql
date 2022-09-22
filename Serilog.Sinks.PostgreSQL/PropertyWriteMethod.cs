namespace Serilog.Sinks.PostgreSql
{
	public enum PropertyWriteMethod
	{
		Raw = 0,
		ToString = 1,
		Json = 2
	}
}
﻿using System;

namespace Serilog.Sinks.PostgreSql.IntegrationTests.Objects
{
	public class TestObjectType2
	{
		public DateTime DateProp1 { get; set; }

		public TestObjectType1 NestedProp { get; set; }
	}
}
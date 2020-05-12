using System;

namespace ZyRabbit.Pipe.Middleware
{
	public class MiddlewareInfo
	{
		public Type Type { get; set; }
		public object[] ConstructorArgs { get; set; }
	}
}

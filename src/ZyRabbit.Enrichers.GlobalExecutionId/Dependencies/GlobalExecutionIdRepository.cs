using System.Threading;

namespace ZyRabbit.Enrichers.GlobalExecutionId.Dependencies
{
	public class GlobalExecutionIdRepository
	{
		private static readonly AsyncLocal<string> GlobalExecutionId = new AsyncLocal<string>();

		public static string Get()
		{
			return GlobalExecutionId?.Value;
		}

		public static void Set(string id)
		{
			GlobalExecutionId.Value = id;
		}
	}
}

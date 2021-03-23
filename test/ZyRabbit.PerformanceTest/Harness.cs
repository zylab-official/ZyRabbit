using System;
using BenchmarkDotNet.Running;
using Xunit;

namespace ZyRabbit.PerformanceTest
{
	public sealed class Harness
	{
		[Fact]
		public void PubSubBenchmarks()
		{
			var result = BenchmarkRunner.Run<PubSubBenchmarks>();
			Assert.Empty(result.ValidationErrors);
			Assert.NotEqual(TimeSpan.Zero, result.TotalTime);
		}

		[Fact]
		public void RpcBenchmarks()
		{
			var result = BenchmarkRunner.Run<RpcBenchmarks>();
			Assert.Empty(result.ValidationErrors);
			Assert.NotEqual(TimeSpan.Zero, result.TotalTime);
		}

		[Fact]
		public void MessageContextBenchmarks()
		{
			var result = BenchmarkRunner.Run<MessageContextBenchmarks>();
			Assert.Empty(result.ValidationErrors);
			Assert.NotEqual(TimeSpan.Zero, result.TotalTime);
		}
	}
}

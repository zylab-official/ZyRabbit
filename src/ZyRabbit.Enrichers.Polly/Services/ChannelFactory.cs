using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using ZyRabbit.Configuration;

namespace ZyRabbit.Enrichers.Polly.Services
{
	public class ChannelFactory : Channel.ChannelFactory
	{
		protected readonly Policy CreateChannelPolicy;
		protected readonly Policy ConnectPolicy;
		protected readonly Policy GetConnectionPolicy;

		public ChannelFactory(IConnectionFactory connectionFactory, ZyRabbitConfiguration config, ILogger<Channel.ChannelFactory> logger, ConnectionPolicies policies = null)
			: base(connectionFactory, config, logger)
		{
			CreateChannelPolicy = policies?.CreateChannel ?? Policy.NoOpAsync();
			ConnectPolicy = policies?.Connect ?? Policy.NoOpAsync();
			GetConnectionPolicy = policies?.GetConnection ?? Policy.NoOpAsync();
		}

		public override Task ConnectAsync(CancellationToken token = default(CancellationToken))
		{
			return ConnectPolicy.ExecuteAsync(
				action: ct => base.ConnectAsync(ct),
				contextData: new Dictionary<string, object>
				{
					[RetryKey.ConnectionFactory] = ConnectionFactory,
					[RetryKey.ClientConfiguration] = ClientConfig
				},
				cancellationToken: token
			);
		}

		protected override Task<IConnection> GetConnectionAsync(CancellationToken token = default(CancellationToken))
		{
			return GetConnectionPolicy.ExecuteAsync(
				action: ct => base.GetConnectionAsync(ct),
				contextData: new Dictionary<string, object>
				{
					[RetryKey.ConnectionFactory] = ConnectionFactory,
					[RetryKey.ClientConfiguration] = ClientConfig
				},
				cancellationToken: token
			);
		}

		public override Task<IModel> CreateChannelAsync(CancellationToken token = default(CancellationToken))
		{
			return CreateChannelPolicy.ExecuteAsync(
				action: ct => base.CreateChannelAsync(ct),
				contextData: new Dictionary<string, object>
				{
					[RetryKey.ConnectionFactory] = ConnectionFactory,
					[RetryKey.ClientConfiguration] = ClientConfig
				},
				cancellationToken: token
			);
		}
	}

	public class ConnectionPolicies
	{
		/// <summary>
		/// Used whenever 'CreateChannelAsync' is called.
		/// Expects an async policy.
		/// </summary>
		public Policy CreateChannel { get; set; }

		/// <summary>
		/// Used whenever an existing connection is retrieved.
		/// </summary>
		public Policy GetConnection { get; set; }

		/// <summary>
		/// Used when establishing the initial connection
		/// </summary>
		public Policy Connect { get; set; }
	}
}

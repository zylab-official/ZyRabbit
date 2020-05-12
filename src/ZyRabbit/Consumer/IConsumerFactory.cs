using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using ZyRabbit.Configuration.Consume;

namespace ZyRabbit.Consumer
{
	public interface IConsumerFactory
	{
		Task<IBasicConsumer> GetConsumerAsync(ConsumeConfiguration cfg, IModel channel = null, CancellationToken token = default(CancellationToken));
		Task<IBasicConsumer> CreateConsumerAsync(IModel channel = null, CancellationToken token = default(CancellationToken));
		IBasicConsumer ConfigureConsume(IBasicConsumer consumer, ConsumeConfiguration cfg);
		Task<IBasicConsumer> GetConfiguredConsumerAsync(ConsumeConfiguration cfg, IModel channel = null, CancellationToken token = default(CancellationToken));
	}
}

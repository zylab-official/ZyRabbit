using System.Threading.Tasks;
using Ninject;
using ZyRabbit.DependencyInjection.Ninject;
using ZyRabbit.Instantiation;
using Xunit;

namespace ZyRabbit.IntegrationTests.DependencyInjection
{
	public class NinjectTests
	{
		[Fact]
		public async Task Should_Be_Able_To_Resolve_Client_From_Ninject()
		{
			/* Setup */
			var kernel = new StandardKernel();
			kernel.RegisterZyRabbit();

			/* Test */
			var client = kernel.Get<IBusClient>();
			var instanceFactory = kernel.Get<IInstanceFactory>();

			/* Assert */
			(instanceFactory as InstanceFactory)?.Dispose();
			Assert.NotNull(client);
		}
	}
}

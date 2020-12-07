using System.Threading;

namespace ZyRabbit.Enrichers.MessageContext.Dependencies
{
	public interface IMessageContextRepository
	{
		object Get();
		void Set(object context);
	}

	public class MessageContextRepository : IMessageContextRepository
	{

		private readonly AsyncLocal<object> _msgContext;

		public MessageContextRepository()
		{
			_msgContext = new AsyncLocal<object>();
		}
		public object Get()
		{
			return _msgContext?.Value;
		}

		public void Set(object context)
		{
			_msgContext.Value = context;
		}
	}
}

using System.Threading;

#if NET451
using System.Runtime.Remoting.Messaging;
#endif

namespace ZyRabbit.Enrichers.MessageContext.Dependencies
{
	public interface IMessageContextRepository
	{
		object Get();
		void Set(object context);
	}

	public class MessageContextRepository : IMessageContextRepository
	{

#if NETSTANDARD2_0
		private readonly AsyncLocal<object> _msgContext;
#elif NET451
		private const string MessageContext = "ZyRabbit:MessageContext";
#endif

		public MessageContextRepository()
		{
#if NETSTANDARD2_0
			_msgContext = new AsyncLocal<object>();
#endif
		}
		public object Get()
		{
#if NETSTANDARD2_0
			return _msgContext?.Value;
#elif NET451
			return CallContext.LogicalGetData(MessageContext) as object;
#endif
		}

		public void Set(object context)
		{
#if NETSTANDARD2_0
			_msgContext.Value = context;
#elif NET451
			CallContext.LogicalSetData(MessageContext, context);
#endif
		}
	}
}

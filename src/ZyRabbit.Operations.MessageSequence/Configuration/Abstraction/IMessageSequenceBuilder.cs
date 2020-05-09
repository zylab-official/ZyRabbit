using System;
using System.Threading.Tasks;

namespace ZyRabbit.Operations.MessageSequence.Configuration.Abstraction
{
	public interface IMessageSequenceBuilder
	{
		IMessageSequenceBuilder When<TMessage, TMessageContext>(Func<TMessage, TMessageContext, Task> func, Action<IStepOptionBuilder> options = null);
		Model.MessageSequence<TMessage> Complete<TMessage>();
	}
}

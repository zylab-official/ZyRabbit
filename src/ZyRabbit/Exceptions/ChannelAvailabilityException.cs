using System;

namespace ZyRabbit.Exceptions
{
	public class ChannelAvailabilityException : Exception
	{
		public ChannelAvailabilityException(string message) : base(message)
		{ }
	}
}

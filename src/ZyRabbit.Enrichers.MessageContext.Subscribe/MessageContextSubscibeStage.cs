namespace ZyRabbit.Enrichers.MessageContext.Subscribe
{
	public enum MessageContextSubscibeStage
	{
		MessageReceived,
		MessageDeserialized,
		MessageContextDeserialized,
		MessageContextEnhanced,
		HandlerInvoked
	}
}

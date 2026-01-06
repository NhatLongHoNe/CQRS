public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent evt);
    // Task PublishAsync<TEvent>(TEvent @event) where TEvent : notnull;
}
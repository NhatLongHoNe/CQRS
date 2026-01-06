public class ConsoleEventPublisher : IEventPublisher
{
    public Task PublishAsync<TEvent>(TEvent evt)
    {
        Console.WriteLine($"--> Publishing event: {evt}");
        return Task.CompletedTask;
    }
}
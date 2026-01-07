public interface IEventHandler<TEvent>
{
    Task HandleAsync(TEvent evt);
}
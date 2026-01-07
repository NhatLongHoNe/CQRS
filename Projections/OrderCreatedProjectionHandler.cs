using MediatR;

public class OrderCreatedProjectionHandler : INotificationHandler<OrderCreatedEvent>
{
    private readonly ReadDbContext _db;
    public OrderCreatedProjectionHandler(ReadDbContext db)
    {
        _db = db;
    }
    public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        var order = new Order
        {
            Id = notification.OrderId,
            FirstName = notification.FirstName,
            LastName = notification.LastName,
            TotalCost = notification.TotalCost,
            Status = "Created",
            CreatedAt = DateTime.UtcNow
        };
        await _db.Orders.AddAsync(order);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
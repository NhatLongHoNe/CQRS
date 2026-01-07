public class OrderCreatedProjectionHandler : IEventHandler<OrderCreatedEvent>
{
    private readonly ReadDbContext _db;
    public OrderCreatedProjectionHandler(ReadDbContext db)
    {
        _db = db;
    }
    public async Task HandleAsync(OrderCreatedEvent evt)
    {
        var order = new Order
        {
            Id = evt.OrderId,
            FirstName = evt.FirstName,
            LastName = evt.LastName,
            TotalCost = evt.TotalCost,
            Status = "Created",
            CreatedAt = DateTime.UtcNow
        };
        await _db.Orders.AddAsync(order);
        await _db.SaveChangesAsync();
    }
}
public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, OrderDto>
{
    private readonly AppDbContext _db;
    public CreateOrderCommandHandler(AppDbContext db)
    {
        _db = db;
    }
    public async Task<OrderDto> HandleAsync(CreateOrderCommand command)
    {
        var order = new Order
        {
            FirstName = command.FirstName,
            LastName = command.LastName,
            Status = command.Status,
            TotalCost = command.TotalCost,
            CreatedAt = DateTime.UtcNow
        };
        await _db.Orders.AddAsync(order);
        await _db.SaveChangesAsync();
        return new OrderDto(order.Id, order.FirstName, order.LastName, order.Status, order.CreatedAt, order.TotalCost);
    }
    // public static async Task<Order> Handle(CreateOrderCommand command, AppDbContext db)
    // {
    //     var order = new Order
    //     {
    //         FirstName = command.FirstName,
    //         LastName = command.LastName,
    //         Status = command.Status,
    //         TotalCost = command.TotalCost,
    //         CreatedAt = DateTime.UtcNow
    //     };
    //     await db.Orders.AddAsync(order);
    //     await db.SaveChangesAsync();
    //     return order;
    // }
}
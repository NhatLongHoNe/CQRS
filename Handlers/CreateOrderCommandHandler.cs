public class CreateOrderCommandHandler
{
    public static async Task<Order> Handle(CreateOrderCommand command, AppDbContext db)
    {
        var order = new Order
        {
            FirstName = command.FirstName,
            LastName = command.LastName,
            Status = command.Status,
            TotalCost = command.TotalCost,
            CreatedAt = DateTime.UtcNow
        };
        await db.Orders.AddAsync(order);
        await db.SaveChangesAsync();
        return order;
    }
}
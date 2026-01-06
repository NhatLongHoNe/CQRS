using Microsoft.EntityFrameworkCore;

public class GetOrderByIdQueryHandler
{
    public static async Task<Order?> Handle(GetOrderByIdQuery query, AppDbContext db)
    {
        return await db.Orders.FirstOrDefaultAsync(o => o.Id == query.OrderId);
    }
}
using Microsoft.EntityFrameworkCore;

public class GetOrderByIdQueryHandler : IQueryHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly AppDbContext _db;
    public GetOrderByIdQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<OrderDto> HandleAsync(GetOrderByIdQuery query)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == query.OrderId);
        if (order == null) {
            return null;
        }
        return new OrderDto(
            order.Id, 
            order.FirstName, 
            order.LastName, 
            order.Status, 
            order.CreatedAt, 
            order.TotalCost
        );
    }

    // public static async Task<Order?> Handle(GetOrderByIdQuery query, AppDbContext db)
    // {
    //     return await db.Orders.FirstOrDefaultAsync(o => o.Id == query.OrderId);
    // }
}
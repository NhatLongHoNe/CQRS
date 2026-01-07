using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly ReadDbContext _db;
    public GetOrderByIdQueryHandler(ReadDbContext db)
    {
        _db = db;
    }

    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _db.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);
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
using Microsoft.EntityFrameworkCore;

public class GetOrderSummariesQueryHandler : IQueryHandler<GetOrderSummariesQuery, List<OrderSummaryDto>>{
    private readonly AppDbContext _db;
    public GetOrderSummariesQueryHandler(AppDbContext db)
    {
        _db = db;
    }
    public async Task<List<OrderSummaryDto>> HandleAsync(GetOrderSummariesQuery query)
    {
        var orders = await _db.Orders
            .Select(o => new OrderSummaryDto(o.Id, o.FirstName + " " + o.LastName, o.Status, o.TotalCost))
            .ToListAsync();
        return orders;
    }
}
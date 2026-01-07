using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetOrderSummariesQueryHandler : IRequestHandler<GetOrderSummariesQuery, List<OrderSummaryDto>>{
    private readonly ReadDbContext _db;
    public GetOrderSummariesQueryHandler(ReadDbContext db)
    {
        _db = db;
    }
    public async Task<List<OrderSummaryDto>> Handle(GetOrderSummariesQuery request, CancellationToken cancellationToken)
    {
        var orders = await _db.Orders
            .AsNoTracking()
            .Select(o => new OrderSummaryDto(o.Id, o.FirstName + " " + o.LastName, o.Status, o.TotalCost))
            .ToListAsync(cancellationToken);
        return orders;
    }
}
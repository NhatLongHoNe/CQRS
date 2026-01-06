using FluentValidation;

public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, OrderDto>
{
    private readonly AppDbContext _db;
    private readonly IValidator<CreateOrderCommand> _validator;
    private readonly IEventPublisher _eventPublisher;
    public CreateOrderCommandHandler(AppDbContext db, IValidator<CreateOrderCommand> validator, IEventPublisher eventPublisher)
    {
        _db = db;
        _validator = validator;
        _eventPublisher = eventPublisher;
    }
    public async Task<OrderDto> HandleAsync(CreateOrderCommand command)
    {
        var validationResult = await _validator.ValidateAsync(command);
        if (!validationResult.IsValid) {
            throw new ValidationException(validationResult.Errors);
        }
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

        await _eventPublisher.PublishAsync(new OrderCreatedEvent(order.Id, order.FirstName, order.LastName, order.TotalCost));

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
using FluentValidation;
using MediatR;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly WriteDbContext _db;
    private readonly IValidator<CreateOrderCommand> _validator;
    //private readonly IEventPublisher _eventPublisher;
    private readonly IMediator _mediator;
    public CreateOrderCommandHandler(
        WriteDbContext db, 
        IValidator<CreateOrderCommand> validator,
        IMediator mediator
       // IEventPublisher eventPublisher
        )
    {
        _db = db;
        _validator = validator;
       // _eventPublisher = eventPublisher;
        _mediator = mediator;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid) {
            throw new ValidationException(validationResult.Errors);
        }
        var order = new Order
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Status = request.Status,
            TotalCost = request.TotalCost,
            CreatedAt = DateTime.UtcNow
        };
        await _db.Orders.AddAsync(order);
        await _db.SaveChangesAsync(cancellationToken);

        await _mediator.Publish(new OrderCreatedEvent(order.Id, order.FirstName, order.LastName, order.TotalCost));

        return new OrderDto(order.Id, order.FirstName, order.LastName, order.Status, order.CreatedAt, order.TotalCost);
    }
}
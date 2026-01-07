using Microsoft.EntityFrameworkCore;
using FluentValidation;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

// builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("BaseConnection")));
builder.Services.AddDbContext<WriteDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("WriteDbConnection")));
builder.Services.AddDbContext<ReadDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("ReadDbConnection")));

// builder.Services.AddScoped<ICommandHandler<CreateOrderCommand, OrderDto>, CreateOrderCommandHandler>();
// builder.Services.AddScoped<IQueryHandler<GetOrderByIdQuery, OrderDto>, GetOrderByIdQueryHandler>();
// builder.Services.AddScoped<IQueryHandler<GetOrderSummariesQuery, List<OrderSummaryDto>>, GetOrderSummariesQueryHandler>();
builder.Services.AddScoped<IValidator<CreateOrderCommand>, CreateOrderCommandValidator>();

// builder.Services.AddSingleton<IEventPublisher, ConsoleEventPublisher>();
// builder.Services.AddSingleton<IEventPublisher, InProcessEventPublisher>();
// builder.Services.AddScoped<IEventHandler<OrderCreatedEvent>, OrderCreatedProjectionHandler>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

var app = builder.Build();

app.MapPost("/api/orders", async (IMediator mediator, CreateOrderCommand command) => {
    // db.Orders.Add(order);
    // db.SaveChanges();
    // var order = await CreateOrderCommandHandler.Handle(command, db);
    try {
        var order = await mediator.Send(command);
        if (order == null) {
            return Results.BadRequest();
        }

        return Results.Created($"/api/orders/{order.Id}", order);
    }catch (ValidationException ex) {
        var errors = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
        return Results.BadRequest(errors);
    }
    
});
app.MapGet("/api/orders/{id}", async (IMediator mediator, int id) => {
    // var order = db.Orders.Find(id);
    // var order = await GetOrderByIdQueryHandler.Handle(new GetOrderByIdQuery(id), db);
    var order = await mediator.Send(new GetOrderByIdQuery(id));
    if (order == null) {
        return Results.NotFound();
    }
    return Results.Ok(order);
});

app.MapGet("/api/orders", async (IMediator mediator) => {
    var summaries = await mediator.Send(new GetOrderSummariesQuery());
    return Results.Ok(summaries);
});


app.Run();

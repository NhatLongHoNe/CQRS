using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("BaseConnection")));

builder.Services.AddScoped<ICommandHandler<CreateOrderCommand, OrderDto>, CreateOrderCommandHandler>();
builder.Services.AddScoped<IQueryHandler<GetOrderByIdQuery, OrderDto>, GetOrderByIdQueryHandler>();

var app = builder.Build();

app.MapPost("/api/orders", async (ICommandHandler<CreateOrderCommand, OrderDto> handler, CreateOrderCommand command) => {
    // db.Orders.Add(order);
    // db.SaveChanges();
    // var order = await CreateOrderCommandHandler.Handle(command, db);
    var order = await handler.HandleAsync(command);
    if (order == null) {
        return Results.BadRequest();
    }
    return Results.Created($"/api/orders/{order.Id}", order);
});
app.MapGet("/api/orders/{id}", async (IQueryHandler<GetOrderByIdQuery, OrderDto> handler, int id) => {
    // var order = db.Orders.Find(id);
    // var order = await GetOrderByIdQueryHandler.Handle(new GetOrderByIdQuery(id), db);
    var order = await handler.HandleAsync(new GetOrderByIdQuery(id));
    if (order == null) {
        return Results.NotFound();
    }
    return Results.Ok(order);
});

app.MapGet("/api/orders", (AppDbContext db) => {
    return db.Orders.ToList();
});

app.Run();

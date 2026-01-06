using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("BaseConnection")));

var app = builder.Build();

app.MapPost("/api/orders", async (AppDbContext db, CreateOrderCommand command) => {
    // db.Orders.Add(order);
    // db.SaveChanges();
    var order = await CreateOrderCommandHandler.Handle(command, db);
    if (order == null) {
        return Results.BadRequest();
    }
    return Results.Created($"/api/orders/{order.Id}", order);
});
app.MapGet("/api/orders/{id}", async (AppDbContext db, int id) => {
    // var order = db.Orders.Find(id);
    var order = await GetOrderByIdQueryHandler.Handle(new GetOrderByIdQuery(id), db);
    if (order == null) {
        return Results.NotFound();
    }
    return Results.Ok(order);
});

app.MapGet("/api/orders", (AppDbContext db) => {
    return db.Orders.ToList();
});

app.Run();

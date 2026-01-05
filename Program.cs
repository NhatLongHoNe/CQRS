using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("BaseConnection")));

var app = builder.Build();

app.MapPost("/api/orders", (AppDbContext db, Order order) => {
    db.Orders.Add(order);
    db.SaveChanges();
    return Results.Created($"/api/orders/{order.Id}", order);
});

app.MapGet("/api/orders/{id}", (AppDbContext db, int id) => {
    var order = db.Orders.Find(id);
    if (order == null) {
        return Results.NotFound();
    }
    return Results.Ok(order);
});

app.MapGet("/api/orders", (AppDbContext db) => {
    return db.Orders.ToList();
});

app.Run();

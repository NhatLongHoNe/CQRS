public class Order
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Status { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required decimal TotalCost { get; set; }
}
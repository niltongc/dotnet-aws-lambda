namespace Model;

public class User
{
    public Guid Id { get; init; }
    public required string Name { get; set; }
    public required string Email { get; set; }
}
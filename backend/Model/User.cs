namespace Model;

public class User
{
    public Guid Id { get; init; }
    public required string Name { get; set; }
    public required string Email { get; set; }
}

public class UserDto
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string Email { get; set; }
}
namespace AfneyGym.Domain.Entities;

public class Gym : BaseEntity
{
    public required string Name { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public virtual ICollection<User>? Members { get; set; }
}
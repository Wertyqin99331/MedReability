namespace MedReability.Domain.Entities;

public class Clinic
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public List<User> Users { get; set; } = [];
}

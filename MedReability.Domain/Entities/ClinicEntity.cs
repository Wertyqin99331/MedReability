namespace MedReability.Domain.Entities;

public class ClinicEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public List<UserEntity> Users { get; set; } = [];
}

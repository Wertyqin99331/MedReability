using MedReability.Domain.Enums;

namespace MedReability.Domain.Entities;

public class Exercise
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Clinic Clinic { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
    public Guid? UserId { get; set; }
    public User? User { get; set; }
    public bool IsDeleted { get; set; }
    public ExerciseType Type { get; set; }
    public string[] Steps { get; set; } = [];
}

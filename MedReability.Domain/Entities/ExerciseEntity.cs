using MedReability.Domain.Enums;

namespace MedReability.Domain.Entities;

public class ExerciseEntity
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public ClinicEntity ClinicEntity { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string[] MediaUrls { get; set; } = [];
    public Guid? UserId { get; set; }
    public UserEntity? UserEntity { get; set; }
    public bool IsDeleted { get; set; }
    public ExerciseType Type { get; set; }
    public string[] Steps { get; set; } = [];
}

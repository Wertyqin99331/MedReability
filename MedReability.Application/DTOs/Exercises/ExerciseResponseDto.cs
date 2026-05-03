using MedReability.Domain.Enums;

namespace MedReability.Application.DTOs.Exercises;

public class ExerciseResponseDto
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid? UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> MediaUrls { get; set; } = [];
    public bool IsDeleted { get; set; }
    public ExerciseType Type { get; set; }
    public List<string> ExerciseTypes { get; set; } = [];
    public List<string> BodyParts { get; set; } = [];
    public List<string> Inventory { get; set; } = [];
    public List<string> Steps { get; set; } = [];
}

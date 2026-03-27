using MedReability.Domain.Enums;

namespace MedReability.Application.DTOs.Exercises;

public class ExerciseResponseDto
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid? UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
    public bool IsDeleted { get; set; }
    public ExerciseType Type { get; set; }
    public List<string> Steps { get; set; } = [];
}

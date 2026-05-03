using MedReability.Application.DTOs.Exercises;

namespace MedReability.Application.DTOs.Assignments;

public class DoctorPatientTodayWorkoutExerciseDto
{
    public int Order { get; set; }
    public ExerciseResponseDto ExerciseEntity { get; set; } = null!;
    public int? Sets { get; set; }
    public int? RestBetweenSetsInSeconds { get; set; }
    public int? RestAfterInSeconds { get; set; }
    public int? Repetitions { get; set; }
    public int? DurationSeconds { get; set; }
    public string? Comment { get; set; }
}

using MedReability.Application.DTOs.Exercises;

namespace MedReability.Application.DTOs.Assignments;

public class DoctorPatientTodayWorkoutExerciseDto
{
    public int Order { get; set; }
    public ExerciseResponseDto Exercise { get; set; } = null!;
    public int? Repetitions { get; set; }
    public int? DurationSeconds { get; set; }
    public string? Comment { get; set; }
}

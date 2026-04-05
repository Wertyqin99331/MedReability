namespace MedReability.Application.DTOs.Assignments;

public class DoctorPatientTodayWorkoutDto
{
    public DateOnly Date { get; set; }
    public int DayNumber { get; set; }
    public bool IsCompletedToday { get; set; }
    public bool IsRestDay { get; set; }
    public List<DoctorPatientTodayWorkoutExerciseDto> Exercises { get; set; } = [];
}

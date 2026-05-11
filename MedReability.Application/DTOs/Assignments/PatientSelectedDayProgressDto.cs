namespace MedReability.Application.DTOs.Assignments;

public class PatientSelectedDayProgressDto
{
    public int? WellBeingRating { get; set; }
    public int? WorkoutDifficultyRating { get; set; }
    public bool? HadPain { get; set; }
    public int? PainIntensityRating { get; set; }
}

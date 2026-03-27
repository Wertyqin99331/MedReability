namespace MedReability.Application.DTOs.Exercises;

public class ListExercisesQueryDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public bool All { get; set; }
}

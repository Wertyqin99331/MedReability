using MedReability.Application.DTOs.Common;
using MedReability.Application.DTOs.Exercises;

namespace MedReability.Application.Interfaces.Services;

public interface IExerciseService
{
    Task<ExerciseResponseDto> CreateExerciseAsync(
        Guid clinicId,
        Guid? userId,
        CreateExerciseRequestDto request,
        CancellationToken cancellationToken = default);

    Task<PagedResultDto<ExerciseResponseDto>> GetExercisesAsync(
        Guid clinicId,
        Guid userId,
        ListExercisesQueryDto query,
        CancellationToken cancellationToken = default);

    Task<ExerciseResponseDto> GetExerciseByIdAsync(
        Guid clinicId,
        Guid userId,
        Guid exerciseId,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    Task<ExerciseResponseDto> UpdateExerciseAsync(
        Guid clinicId,
        Guid userId,
        Guid exerciseId,
        bool isAdmin,
        UpdateExerciseRequestDto request,
        CancellationToken cancellationToken = default);

    Task SoftDeleteExerciseAsync(
        Guid clinicId,
        Guid userId,
        Guid exerciseId,
        bool isAdmin,
        CancellationToken cancellationToken = default);
}

using MedReability.Application.DTOs.TrainingPlans;

namespace MedReability.Application.Interfaces.Services;

public interface IPatientTrainingPlanService
{
    Task<PatientTrainingPlanResponseDto> CreateAsync(
        Guid clinicId,
        Guid currentUserId,
        bool isAdmin,
        CreatePatientTrainingPlanRequestDto request,
        CancellationToken cancellationToken = default);

    Task<PatientTrainingPlanResponseDto> UpdateAsync(
        Guid clinicId,
        Guid currentUserId,
        bool isAdmin,
        Guid planId,
        UpdatePatientTrainingPlanRequestDto request,
        CancellationToken cancellationToken = default);

    Task<PatientTrainingPlanDayProgressResponseDto> CompleteDayAsync(
        Guid clinicId,
        Guid patientId,
        Guid planId,
        int dayNumber,
        CancellationToken cancellationToken = default);

    Task<PatientTrainingPlanDayProgressResponseDto> UpdateDayProgressAsync(
        Guid clinicId,
        Guid patientId,
        Guid planId,
        int dayNumber,
        UpdatePatientTrainingPlanDayProgressRequestDto request,
        CancellationToken cancellationToken = default);
}

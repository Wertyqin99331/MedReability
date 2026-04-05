using MedReability.Api.Auth;
using MedReability.Api.Common;
using MedReability.Application.DTOs.TrainingPlans;
using MedReability.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace MedReability.Api.Controllers;

[ApiController]
[PatientOnly]
[Route("api/patient/program-progress")]
public class PatientProgramProgressController(IPatientTrainingPlanService trainingPlanService) : ControllerBase
{
    [HttpPost("{planId:guid}/days/{dayNumber:int}/complete")]
    [ProducesResponseType(typeof(PatientTrainingPlanDayProgressResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteDay(
        Guid planId,
        int dayNumber,
        CancellationToken cancellationToken)
    {
        var clinicId = User.GetClinicId();
        var userId = User.GetUserId();

        if (clinicId is null || userId is null)
        {
            return Forbid();
        }

        var result = await trainingPlanService.CompleteDayAsync(
            clinicId.Value,
            userId.Value,
            planId,
            dayNumber,
            cancellationToken);

        return Ok(result);
    }

    [HttpPatch("{planId:guid}/days/{dayNumber:int}/progress")]
    [ProducesResponseType(typeof(PatientTrainingPlanDayProgressResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDayProgress(
        Guid planId,
        int dayNumber,
        [FromBody] UpdatePatientTrainingPlanDayProgressRequestDto request,
        CancellationToken cancellationToken)
    {
        var clinicId = User.GetClinicId();
        var userId = User.GetUserId();

        if (clinicId is null || userId is null)
        {
            return Forbid();
        }

        var result = await trainingPlanService.UpdateDayProgressAsync(
            clinicId.Value,
            userId.Value,
            planId,
            dayNumber,
            request,
            cancellationToken);

        return Ok(result);
    }
}

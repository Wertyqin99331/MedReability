using MedReability.Api.Auth;
using MedReability.Api.Common;
using MedReability.Application.DTOs.TrainingPlans;
using MedReability.Application.Interfaces.Services;
using MedReability.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace MedReability.Api.Controllers;

[ApiController]
[AdminOrDoctor]
[Route("api/programs")]
public class ProgramsController(IPatientTrainingPlanService trainingPlanService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PatientTrainingPlanResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var clinicId = User.GetClinicId();
        var userId = User.GetUserId();

        if (clinicId is null || userId is null)
        {
            return Forbid();
        }

        var result = await trainingPlanService.GetByIdAsync(
            clinicId.Value,
            userId.Value,
            User.IsInRole(nameof(UserRole.Admin)),
            id,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(PatientTrainingPlanResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePatientTrainingPlanRequestDto request,
        CancellationToken cancellationToken)
    {
        var clinicId = User.GetClinicId();
        var userId = User.GetUserId();

        if (clinicId is null || userId is null)
        {
            return Forbid();
        }

        var result = await trainingPlanService.CreateAsync(
            clinicId.Value,
            userId.Value,
            User.IsInRole(nameof(UserRole.Admin)),
            request,
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PatientTrainingPlanResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdatePatientTrainingPlanRequestDto request,
        CancellationToken cancellationToken)
    {
        var clinicId = User.GetClinicId();
        var userId = User.GetUserId();

        if (clinicId is null || userId is null)
        {
            return Forbid();
        }

        var result = await trainingPlanService.UpdateAsync(
            clinicId.Value,
            userId.Value,
            User.IsInRole(nameof(UserRole.Admin)),
            id,
            request,
            cancellationToken);

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var clinicId = User.GetClinicId();
        var userId = User.GetUserId();

        if (clinicId is null || userId is null)
        {
            return Forbid();
        }

        await trainingPlanService.DeleteAsync(
            clinicId.Value,
            userId.Value,
            User.IsInRole(nameof(UserRole.Admin)),
            id,
            cancellationToken);

        return NoContent();
    }
}

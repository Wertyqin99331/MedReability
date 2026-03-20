using MedReability.Api.Auth;
using MedReability.Api.Common;
using MedReability.Application.DTOs.Assignments;
using MedReability.Application.DTOs.Common;
using MedReability.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace MedReability.Api.Controllers;

[ApiController]
[AdminOnly]
[Route("api/doctor-patient-assignments")]
public class DoctorPatientAssignmentsController(
    IDoctorPatientAssignmentService assignmentService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<DoctorPatientAssignmentListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAssignments(
        [FromQuery] DoctorPatientAssignmentsQueryDto query,
        CancellationToken cancellationToken)
    {
        var clinicId = User.GetClinicId();
        if (clinicId is null)
        {
            return Forbid();
        }

        var result = await assignmentService.GetAssignmentsAsync(clinicId.Value, query, cancellationToken);
        return Ok(result);
    }

    [HttpPost("assign-doctor-to-patient")]
    [ProducesResponseType(typeof(DoctorPatientAssignmentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignDoctorToPatient(
        [FromBody] AssignDoctorToPatientRequestDto request,
        CancellationToken cancellationToken)
    {
        var clinicId = User.GetClinicId();
        if (clinicId is null)
        {
            return Forbid();
        }

        var result = await assignmentService.AssignAsync(
            clinicId.Value,
            request.DoctorId,
            request.PatientId,
            cancellationToken);

        return CreatedAtAction(nameof(AssignDoctorToPatient), new { id = result.Id }, result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAssignment(Guid id, CancellationToken cancellationToken)
    {
        var clinicId = User.GetClinicId();
        if (clinicId is null)
        {
            return Forbid();
        }

        var deleted = await assignmentService.DeleteAsync(clinicId.Value, id, cancellationToken);
        if (!deleted)
        {
            return Problem(
                title: "Assignment not found",
                detail: "Assignment was not found in your clinic.",
                statusCode: StatusCodes.Status404NotFound);
        }

        return NoContent();
    }
}

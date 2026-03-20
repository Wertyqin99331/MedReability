using MedReability.Api.Auth;
using MedReability.Api.Common;
using MedReability.Application.DTOs.Assignments;
using MedReability.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace MedReability.Api.Controllers;

[ApiController]
[DoctorOnly]
[Route("api/doctors")]
public class DoctorsController(
    IDoctorPatientAssignmentService assignmentService) : ControllerBase
{
    [HttpGet("me/patients")]
    [ProducesResponseType(typeof(List<DoctorPatientListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyPatients(CancellationToken cancellationToken)
    {
        var clinicId = User.GetClinicId();
        var doctorId = User.GetUserId();

        if (clinicId is null || doctorId is null)
        {
            return Forbid();
        }

        var patients = await assignmentService.GetDoctorPatientsAsync(
            clinicId.Value,
            doctorId.Value,
            cancellationToken);

        return Ok(patients);
    }
}

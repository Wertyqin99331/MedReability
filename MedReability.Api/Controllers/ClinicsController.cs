using MedReability.Application.DTOs.Clinics;
using MedReability.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedReability.Api.Controllers;

[ApiController]
[Route("api/clinics")]
public class ClinicsController(IClinicService clinicService) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(List<ClinicResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClinics(CancellationToken cancellationToken)
    {
        var clinics = await clinicService.GetClinicsAsync(cancellationToken);
        return Ok(clinics);
    }
}

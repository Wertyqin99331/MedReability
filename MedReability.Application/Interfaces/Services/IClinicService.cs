using MedReability.Application.DTOs.Clinics;

namespace MedReability.Application.Interfaces.Services;

public interface IClinicService
{
    Task<List<ClinicResponseDto>> GetClinicsAsync(CancellationToken cancellationToken = default);
}

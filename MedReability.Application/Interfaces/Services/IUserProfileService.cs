using MedReability.Application.DTOs.Auth;

namespace MedReability.Application.Interfaces.Services;

public interface IUserProfileService
{
    Task<MeResponseDto> UpdateMyProfileAsync(
        Guid clinicId,
        Guid userId,
        string role,
        UpdateMyProfileRequestDto request,
        CancellationToken cancellationToken = default);
}

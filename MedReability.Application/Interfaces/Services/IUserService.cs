using MedReability.Application.DTOs.Common;
using MedReability.Application.DTOs.Users;

namespace MedReability.Application.Interfaces.Services;

public interface IUserService
{
    Task<UserResponseDto> CreateUserAsync(Guid clinicId, CreateUserRequestDto request, CancellationToken cancellationToken = default);
    Task<PagedResultDto<UserResponseDto>> ListUsersAsync(Guid clinicId, ListUsersQueryDto query, CancellationToken cancellationToken = default);
    Task<UserResponseDto> UpdateUserProfileAsync(Guid clinicId, Guid userId, UpdateUserProfileRequestDto request, CancellationToken cancellationToken = default);
    Task SetUserPasswordAsync(Guid clinicId, Guid userId, SetUserPasswordRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> DeactivateUserAsync(Guid clinicId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ActivateUserAsync(Guid clinicId, Guid userId, CancellationToken cancellationToken = default);
}

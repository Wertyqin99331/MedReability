using MedReability.Application.DTOs.Auth;
using MedReability.Application.Interfaces.Services;
using MedReability.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MedReability.Infrastructure.Services;

public class UserProfileService(AppDbContext dbContext) : IUserProfileService
{
    public async Task<MeResponseDto> UpdateMyProfileAsync(
        Guid clinicId,
        Guid userId,
        string role,
        UpdateMyProfileRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == userId && x.ClinicId == clinicId && x.IsActive, cancellationToken);

        if (user is null)
        {
            throw new KeyNotFoundException("Current user was not found in your clinic.");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var emailTaken = await dbContext.Users
            .AnyAsync(
                x => x.ClinicId == clinicId &&
                     x.Email == normalizedEmail &&
                     x.Id != userId,
                cancellationToken);

        if (emailTaken)
        {
            throw new InvalidOperationException("User with this email already exists in your clinic.");
        }

        user.Email = normalizedEmail;
        user.FirstName = request.FirstName.Trim();
        user.Patronymic = request.Patronymic.Trim();
        user.LastName = request.LastName.Trim();
        user.PhoneNumber = request.PhoneNumber.Trim();

        await dbContext.SaveChangesAsync(cancellationToken);

        return new MeResponseDto
        {
            UserId = user.Id,
            ClinicId = user.ClinicId,
            Email = user.Email,
            Role = role,
            FirstName = user.FirstName,
            Patronymic = user.Patronymic,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber
        };
    }
}

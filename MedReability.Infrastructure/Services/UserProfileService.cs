using MedReability.Application.DTOs.Auth;
using MedReability.Application.Interfaces.Security;
using MedReability.Application.Interfaces.Services;
using MedReability.Application.Interfaces.Storage;
using MedReability.Domain.Entities;
using MedReability.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MedReability.Infrastructure.Services;

public class UserProfileService(
    AppDbContext dbContext,
    IMediaStorageService mediaStorageService,
    IAccessPolicyService accessPolicyService) : IUserProfileService
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public async Task<MeResponseDto> UpdateMyProfileAsync(
        Guid clinicId,
        Guid userId,
        string role,
        UpdateMyProfileRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == userId && x.IsActive, cancellationToken);

        if (user is null || !accessPolicyService.IsSameClinic(clinicId, user.ClinicId))
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

        if (request.Image is not null && request.Image.Length > 0)
        {
            var previousImageUrl = user.ImageUrl;
            user.ImageUrl = await mediaStorageService.UploadAsync("users", request.Image, cancellationToken);

            if (!string.IsNullOrWhiteSpace(previousImageUrl))
            {
                await mediaStorageService.DeleteFileByUrlAsync(previousImageUrl, cancellationToken);
            }
        }

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
            PhoneNumber = user.PhoneNumber,
            ImageUrl = user.ImageUrl
        };
    }

    public async Task ChangePasswordAsync(
        Guid clinicId,
        Guid userId,
        ChangePasswordRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == userId && x.IsActive, cancellationToken);

        if (user is null || !accessPolicyService.IsSameClinic(clinicId, user.ClinicId))
        {
            throw new KeyNotFoundException("Current user was not found in your clinic.");
        }

        if (request.CurrentPassword == request.NewPassword)
        {
            throw new InvalidOperationException("New password must be different from current password.");
        }

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
        if (verification == PasswordVerificationResult.Failed)
        {
            throw new InvalidOperationException("Current password is invalid.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

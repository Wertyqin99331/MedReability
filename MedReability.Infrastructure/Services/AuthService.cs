using MedReability.Application.DTOs.Auth;
using MedReability.Application.Interfaces.Security;
using MedReability.Application.Interfaces.Services;
using MedReability.Domain.Entities;
using MedReability.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MedReability.Infrastructure.Services;

public class AuthService(
    AppDbContext dbContext,
    IJwtTokenGenerator tokenGenerator) : IAuthService
{
    private readonly PasswordHasher<UserEntity> _passwordHasher = new();

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.ClinicId == Guid.Empty)
        {
            return null;
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await dbContext.Users
            .FirstOrDefaultAsync(
                x => x.ClinicId == request.ClinicId && x.Email == normalizedEmail,
                cancellationToken);

        if (user is null || !user.IsActive)
        {
            return null;
        }

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (verification == PasswordVerificationResult.Failed)
        {
            return null;
        }

        var (token, expiresAt) = tokenGenerator.Generate(user);

        return new LoginResponseDto
        {
            AccessToken = token,
            ExpiresAtUtc = expiresAt
        };
    }
}

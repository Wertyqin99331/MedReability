using System.Security.Claims;
using MedReability.Api.Common;
using MedReability.Application.Common;
using MedReability.Application.DTOs.Auth;
using MedReability.Application.Interfaces.Services;
using MedReability.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace MedReability.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    IAuthService authService,
    IUserProfileService userProfileService,
    AppDbContext dbContext) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);

        if (result is null)
        {
            return Problem(
                title: "Invalid credentials",
                detail: "Email or password is invalid, or user is inactive.",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        return Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(MeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");
        var clinicId = User.FindFirstValue(AuthClaimTypes.ClinicId);
        var email = User.FindFirstValue(ClaimTypes.Email)
                    ?? User.FindFirstValue("email");
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (!Guid.TryParse(userId, out var parsedUserId) ||
            !Guid.TryParse(clinicId, out var parsedClinicId) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(role))
        {
            return Unauthorized();
        }

        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == parsedUserId && x.ClinicId == parsedClinicId && x.IsActive,
                cancellationToken);

        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(new MeResponseDto
        {
            UserId = parsedUserId,
            ClinicId = parsedClinicId,
            Email = email,
            Role = role,
            FirstName = user.FirstName,
            Patronymic = user.Patronymic,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber
        });
    }

    [Authorize]
    [HttpPatch("me/profile")]
    [ProducesResponseType(typeof(MeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMyProfile(
        [FromBody] UpdateMyProfileRequestDto request,
        CancellationToken cancellationToken)
    {
        var clinicId = User.GetClinicId();
        var userId = User.GetUserId();
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (clinicId is null || userId is null || string.IsNullOrWhiteSpace(role))
        {
            return Forbid();
        }

        var updatedProfile = await userProfileService.UpdateMyProfileAsync(
            clinicId.Value,
            userId.Value,
            role,
            request,
            cancellationToken);

        return Ok(updatedProfile);
    }
}

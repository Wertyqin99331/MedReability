using MedReability.Api.Common;
using MedReability.Application.DTOs.Common;
using MedReability.Application.DTOs.Users;
using MedReability.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedReability.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/users")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDto request, CancellationToken cancellationToken)
    {
        var clinicId = User.GetClinicId();
        if (clinicId is null)
        {
            return Forbid();
        }

        var user = await userService.CreateUserAsync(clinicId.Value, request, cancellationToken);
        return CreatedAtAction(nameof(GetUsers), new { }, user);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<UserResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUsers([FromQuery] ListUsersQueryDto query, CancellationToken cancellationToken)
    {
        var clinicId = User.GetClinicId();
        if (clinicId is null)
        {
            return Forbid();
        }

        var users = await userService.ListUsersAsync(clinicId.Value, query, cancellationToken);
        return Ok(users);
    }

    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var clinicId = User.GetClinicId();
        if (clinicId is null)
        {
            return Forbid();
        }

        var deactivated = await userService.DeactivateUserAsync(clinicId.Value, id, cancellationToken);
        if (!deactivated)
        {
            return Problem(
                title: "User not found",
                detail: "User was not found in your clinic.",
                statusCode: StatusCodes.Status404NotFound);
        }

        return NoContent();
    }
}

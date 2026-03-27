using MedReability.Api.Auth;
using MedReability.Api.Common;
using MedReability.Api.Models.Exercises;
using MedReability.Api.Storage;
using MedReability.Application.DTOs.Common;
using MedReability.Application.DTOs.Exercises;
using MedReability.Application.Interfaces.Services;
using MedReability.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace MedReability.Api.Controllers;

[ApiController]
[AdminOrDoctor]
[Route("api/exercises")]
public class ExercisesController(
    IExerciseService exerciseService,
    IMediaStorageService mediaStorageService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<ExerciseResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetExercises([FromQuery] ListExercisesQueryDto query, CancellationToken cancellationToken = default)
    {
        var clinicId = User.GetClinicId();
        var userId = User.GetUserId();

        if (clinicId is null || userId is null)
        {
            return Forbid();
        }

        var exercises = await exerciseService.GetExercisesAsync(
            clinicId.Value,
            userId.Value,
            query,
            cancellationToken);

        return Ok(exercises);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ExerciseResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExercise(Guid id, CancellationToken cancellationToken = default)
    {
        var clinicId = User.GetClinicId();
        var userId = User.GetUserId();

        if (clinicId is null || userId is null)
        {
            return Forbid();
        }

        var exercise = await exerciseService.GetExerciseByIdAsync(
            clinicId.Value,
            userId.Value,
            id,
            User.IsInRole(nameof(UserRole.Admin)),
            cancellationToken);

        return Ok(exercise);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ExerciseResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateExercise([FromForm] CreateExerciseFormRequest request, CancellationToken cancellationToken)
    {
        var clinicId = User.GetClinicId();
        var userId = User.GetUserId();

        if (clinicId is null || userId is null)
        {
            return Forbid();
        }

        var mediaUrl = await mediaStorageService.UploadExerciseMediaAsync(request.Media, cancellationToken);

        var createRequest = new CreateExerciseRequestDto
        {
            Name = request.Name,
            Description = request.Description,
            MediaUrl = mediaUrl,
            Steps = request.Steps,
            Type = request.Type,
            IsGlobal = request.IsGlobal
        };

        var exercise = await exerciseService.CreateExerciseAsync(
            clinicId.Value,
            userId.Value,
            createRequest,
            cancellationToken);

        return CreatedAtAction(nameof(CreateExercise), new { id = exercise.Id }, exercise);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteExercise(Guid id, CancellationToken cancellationToken)
    {
        var clinicId = User.GetClinicId();
        var userId = User.GetUserId();

        if (clinicId is null || userId is null)
        {
            return Forbid();
        }

        await exerciseService.SoftDeleteExerciseAsync(
            clinicId.Value,
            userId.Value,
            id,
            User.IsInRole(nameof(UserRole.Admin)),
            cancellationToken);

        return NoContent();
    }
}

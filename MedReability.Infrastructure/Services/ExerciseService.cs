using MedReability.Application.DTOs.Common;
using MedReability.Application.DTOs.Exercises;
using MedReability.Application.Interfaces.Security;
using MedReability.Application.Interfaces.Services;
using MedReability.Application.Interfaces.Storage;
using MedReability.Domain.Entities;
using MedReability.Domain.Enums;
using MedReability.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace MedReability.Infrastructure.Services;

public class ExerciseService(
    AppDbContext dbContext,
    IMediaStorageService mediaStorageService,
    IAccessPolicyService accessPolicyService) : IExerciseService
{
    public async Task<ExerciseResponseDto> CreateExerciseAsync(
        Guid clinicId,
        Guid? userId,
        CreateExerciseRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();
        var description = request.Description.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("ExerciseEntity name is required.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new InvalidOperationException("ExerciseEntity description is required.");
        }

        var stepTexts = request.Steps
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        if (stepTexts.Count == 0)
        {
            throw new InvalidOperationException("At least one exercise step is required.");
        }

        if (!Enum.IsDefined(request.Type))
        {
            throw new InvalidOperationException("ExerciseEntity type is invalid.");
        }

        var exercise = new ExerciseEntity
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            UserId = request.IsGlobal ? null : userId,
            Name = name,
            Description = description,
            MediaUrls = await UploadMediaUrlsAsync(request.MediaFiles, cancellationToken),
            IsDeleted = false,
            Type = request.Type,
            Steps = stepTexts.ToArray()
        };

        dbContext.Exercises.Add(exercise);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(exercise);
    }

    public async Task<PagedResultDto<ExerciseResponseDto>> GetExercisesAsync(
        Guid clinicId,
        Guid userId,
        bool isAdmin,
        ListExercisesQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize switch
        {
            < 1 => 20,
            > 100 => 100,
            _ => query.PageSize
        };

        var exercisesQuery = dbContext.Exercises
            .AsNoTracking()
            .Where(x => x.ClinicId == clinicId && !x.IsDeleted);

        if (!isAdmin)
        {
            exercisesQuery = query.All
                ? exercisesQuery.Where(x => x.UserId == userId || x.UserId == null)
                : exercisesQuery.Where(x => x.UserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLowerInvariant();
            exercisesQuery = exercisesQuery.Where(x =>
                x.Name.ToLower().Contains(search) ||
                x.Description.ToLower().Contains(search));
        }

        if (query.Types is { Count: > 0 })
        {
            exercisesQuery = exercisesQuery.Where(x => query.Types.Contains(x.Type));
        }

        var totalCount = await exercisesQuery.CountAsync(cancellationToken);

        var items = await exercisesQuery
            .OrderBy(x => x.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ExerciseResponseDto
            {
                Id = x.Id,
                ClinicId = x.ClinicId,
                UserId = x.UserId,
                Name = x.Name,
                Description = x.Description,
                MediaUrls = x.MediaUrls.ToList(),
                IsDeleted = x.IsDeleted,
                Type = x.Type,
                Steps = x.Steps.ToList()
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<ExerciseResponseDto>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items
        };
    }

    public async Task<ExerciseResponseDto> GetExerciseByIdAsync(
        Guid clinicId,
        Guid userId,
        Guid exerciseId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var exerciseQuery = dbContext.Exercises
            .AsNoTracking()
            .Where(x => x.Id == exerciseId && x.ClinicId == clinicId && !x.IsDeleted);

        if (!isAdmin)
        {
            exerciseQuery = exerciseQuery.Where(x => x.UserId == userId || x.UserId == null);
        }

        var exercise = await exerciseQuery.FirstOrDefaultAsync(cancellationToken);

        if (exercise is null)
        {
            throw new KeyNotFoundException("ExerciseEntity was not found.");
        }

        return MapToDto(exercise);
    }

    public async Task<ExerciseResponseDto> UpdateExerciseAsync(
        Guid clinicId,
        Guid userId,
        Guid exerciseId,
        bool isAdmin,
        UpdateExerciseRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var exercise = await dbContext.Exercises
            .FirstOrDefaultAsync(x => x.Id == exerciseId && x.ClinicId == clinicId && !x.IsDeleted, cancellationToken);

        if (exercise is null)
        {
            throw new KeyNotFoundException("ExerciseEntity was not found.");
        }

        if (!accessPolicyService.IsAdminOrOwnerOrGlobal(isAdmin, userId, exercise.UserId))
        {
            throw new UnauthorizedAccessException("You are not allowed to edit this exercise.");
        }

        var name = request.Name.Trim();
        var description = request.Description.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("ExerciseEntity name is required.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new InvalidOperationException("ExerciseEntity description is required.");
        }

        var stepTexts = request.Steps
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        if (stepTexts.Count == 0)
        {
            throw new InvalidOperationException("At least one exercise step is required.");
        }

        if (!Enum.IsDefined(request.Type))
        {
            throw new InvalidOperationException("ExerciseEntity type is invalid.");
        }

        exercise.Name = name;
        exercise.Description = description;
        exercise.Type = request.Type;
        exercise.Steps = stepTexts.ToArray();

        if (request.MediaFiles is { Count: > 0 })
        {
            var previousMediaUrls = exercise.MediaUrls;
            exercise.MediaUrls = await UploadMediaUrlsAsync(request.MediaFiles, cancellationToken);

            foreach (var url in previousMediaUrls)
            {
                try
                {
                    await mediaStorageService.DeleteFileByUrlAsync(url, cancellationToken);
                }
                catch
                {
                    // ignore storage cleanup errors; exercise update is primary operation
                }
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return MapToDto(exercise);
    }

    public async Task SoftDeleteExerciseAsync(
        Guid clinicId,
        Guid userId,
        Guid exerciseId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var exercise = await dbContext.Exercises
            .FirstOrDefaultAsync(x => x.Id == exerciseId && x.ClinicId == clinicId && !x.IsDeleted, cancellationToken);

        if (exercise is null)
        {
            throw new KeyNotFoundException("ExerciseEntity was not found.");
        }

        if (!accessPolicyService.IsAdminOrOwnerOrGlobal(isAdmin, userId, exercise.UserId))
        {
            throw new UnauthorizedAccessException("You are not allowed to delete this exercise.");
        }

        exercise.IsDeleted = true;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<string[]> UploadMediaUrlsAsync(List<IFormFile>? mediaFiles, CancellationToken cancellationToken)
    {
        if (mediaFiles is null || mediaFiles.Count == 0)
        {
            return [];
        }

        var uploaded = new List<string>(mediaFiles.Count);
        foreach (var file in mediaFiles)
        {
            var url = await mediaStorageService.UploadAsync("exercises", file, cancellationToken);
            if (!string.IsNullOrWhiteSpace(url))
            {
                uploaded.Add(url);
            }
        }

        return uploaded.ToArray();
    }

    private static ExerciseResponseDto MapToDto(ExerciseEntity exercise)
    {
        return new ExerciseResponseDto
        {
            Id = exercise.Id,
            ClinicId = exercise.ClinicId,
            UserId = exercise.UserId,
            Name = exercise.Name,
            Description = exercise.Description,
            MediaUrls = exercise.MediaUrls.ToList(),
            IsDeleted = exercise.IsDeleted,
            Type = exercise.Type,
            Steps = exercise.Steps.ToList()
        };
    }
}

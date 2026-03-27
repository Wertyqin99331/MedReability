using MedReability.Application.DTOs.Common;
using MedReability.Application.DTOs.Exercises;
using MedReability.Application.Interfaces.Services;
using MedReability.Domain.Entities;
using MedReability.Domain.Enums;
using MedReability.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MedReability.Infrastructure.Services;

public class ExerciseService(AppDbContext dbContext) : IExerciseService
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
            throw new InvalidOperationException("Exercise name is required.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new InvalidOperationException("Exercise description is required.");
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
            throw new InvalidOperationException("Exercise type is invalid.");
        }

        var exercise = new Exercise
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            UserId = request.IsGlobal ? null : userId,
            Name = name,
            Description = description,
            MediaUrl = string.IsNullOrWhiteSpace(request.MediaUrl) ? null : request.MediaUrl.Trim(),
            IsDeleted = false,
            Type = request.Type,
            Steps = stepTexts.ToArray()
        };

        dbContext.Exercises.Add(exercise);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ExerciseResponseDto
        {
            Id = exercise.Id,
            ClinicId = exercise.ClinicId,
            UserId = exercise.UserId,
            Name = exercise.Name,
            Description = exercise.Description,
            MediaUrl = exercise.MediaUrl,
            IsDeleted = exercise.IsDeleted,
            Type = exercise.Type,
            Steps = exercise.Steps.ToList()
        };
    }

    public async Task<PagedResultDto<ExerciseResponseDto>> GetExercisesAsync(
        Guid clinicId,
        Guid userId,
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

        exercisesQuery = query.All
            ? exercisesQuery.Where(x => x.UserId == userId || x.UserId == null)
            : exercisesQuery.Where(x => x.UserId == userId);

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
                MediaUrl = x.MediaUrl,
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
            .Where(x => x.Id == exerciseId
                        && x.ClinicId == clinicId
                        && !x.IsDeleted);

        if (!isAdmin)
        {
            exerciseQuery = exerciseQuery.Where(x => x.UserId == userId || x.UserId == null);
        }

        var exercise = await exerciseQuery
            .Select(x => new ExerciseResponseDto
            {
                Id = x.Id,
                ClinicId = x.ClinicId,
                UserId = x.UserId,
                Name = x.Name,
                Description = x.Description,
                MediaUrl = x.MediaUrl,
                IsDeleted = x.IsDeleted,
                Type = x.Type,
                Steps = x.Steps.ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (exercise is null)
        {
            throw new KeyNotFoundException("Exercise was not found.");
        }

        return exercise;
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
            throw new KeyNotFoundException("Exercise was not found.");
        }

        if (exercise.UserId is not null && exercise.UserId != userId && !isAdmin)
        {
            throw new UnauthorizedAccessException("You are not allowed to delete this exercise.");
        }

        exercise.IsDeleted = true;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

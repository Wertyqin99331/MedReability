using MedReability.Application.DTOs.TrainingPlans;
using MedReability.Application.DTOs.Exercises;
using MedReability.Application.Interfaces.Security;
using MedReability.Application.Interfaces.Services;
using MedReability.Domain.Entities;
using MedReability.Domain.Enums;
using MedReability.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MedReability.Infrastructure.Services;

public class PatientTrainingPlanService(
    AppDbContext dbContext,
    IAccessPolicyService accessPolicyService) : IPatientTrainingPlanService
{
    public async Task<PatientTrainingPlanResponseDto> CreateAsync(
        Guid clinicId,
        Guid currentUserId,
        bool isAdmin,
        CreatePatientTrainingPlanRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await ValidatePatientAsync(clinicId, request.PatientId, cancellationToken);
        await ValidateDoctorAssignmentIfRequiredAsync(clinicId, currentUserId, request.PatientId, isAdmin, cancellationToken);
        await ValidateDaysAsync(clinicId, currentUserId, isAdmin, request.Days, cancellationToken);

        var plan = new PatientTrainingPlan
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            PatientId = request.PatientId,
            CreatedByUserId = currentUserId,
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            StartDate = request.StartDate,
            Status = PatientTrainingPlanStatus.Assigned,
            IsDeleted = false
        };

        plan.Days = request.Days
            .OrderBy(x => x.DayNumber)
            .Select(day => new PatientTrainingPlanDay
            {
                Id = Guid.NewGuid(),
                PatientTrainingPlanId = plan.Id,
                DayNumber = day.DayNumber,
                IsRestDay = day.IsRestDay,
                Notes = string.IsNullOrWhiteSpace(day.Notes) ? null : day.Notes.Trim(),
                Exercises = day.Exercises
                    .OrderBy(x => x.Order)
                    .Select(ex => new PatientTrainingPlanDayExercise
                    {
                        Id = Guid.NewGuid(),
                        Order = ex.Order,
                        ExerciseId = ex.ExerciseId,
                        Repetitions = ex.Repetitions,
                        DurationSeconds = ex.DurationSeconds,
                        Comment = string.IsNullOrWhiteSpace(ex.Comment) ? null : ex.Comment.Trim()
                    })
                    .ToList()
            })
            .ToList();

        dbContext.PatientTrainingPlans.Add(plan);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetMappedPlanAsync(plan.Id, clinicId, cancellationToken);
    }

    public async Task<PatientTrainingPlanResponseDto> UpdateAsync(
        Guid clinicId,
        Guid currentUserId,
        bool isAdmin,
        Guid planId,
        UpdatePatientTrainingPlanRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var plan = await dbContext.PatientTrainingPlans
            .Include(x => x.Days)
            .ThenInclude(x => x.Exercises)
            .FirstOrDefaultAsync(x => x.Id == planId && x.ClinicId == clinicId && !x.IsDeleted, cancellationToken);

        if (plan is null)
        {
            throw new KeyNotFoundException("Training plan was not found.");
        }

        if (!accessPolicyService.IsAdminOrOwner(isAdmin, currentUserId, plan.CreatedByUserId))
        {
            throw new UnauthorizedAccessException("You are not allowed to edit this training plan.");
        }

        await ValidateDoctorAssignmentIfRequiredAsync(clinicId, currentUserId, plan.PatientId, isAdmin, cancellationToken);
        await ValidateDaysAsync(clinicId, currentUserId, isAdmin, request.Days, cancellationToken);

        plan.Name = request.Name.Trim();
        plan.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        plan.StartDate = request.StartDate;

        var existingDayIds = await dbContext.PatientTrainingPlanDays
            .AsNoTracking()
            .Where(x => x.PatientTrainingPlanId == plan.Id)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var exercisesToDelete = dbContext.PatientTrainingPlanDayExercises
            .Where(x => existingDayIds.Contains(x.PatientTrainingPlanDayId));
        dbContext.PatientTrainingPlanDayExercises.RemoveRange(exercisesToDelete);

        var daysToDelete = dbContext.PatientTrainingPlanDays
            .Where(x => x.PatientTrainingPlanId == plan.Id);
        dbContext.PatientTrainingPlanDays.RemoveRange(daysToDelete);

        var newDays = request.Days
            .OrderBy(x => x.DayNumber)
            .Select(day => new PatientTrainingPlanDay
            {
                Id = Guid.NewGuid(),
                PatientTrainingPlanId = plan.Id,
                DayNumber = day.DayNumber,
                IsRestDay = day.IsRestDay,
                Notes = string.IsNullOrWhiteSpace(day.Notes) ? null : day.Notes.Trim(),
                Exercises = day.Exercises
                    .OrderBy(x => x.Order)
                    .Select(ex => new PatientTrainingPlanDayExercise
                    {
                        Id = Guid.NewGuid(),
                        Order = ex.Order,
                        ExerciseId = ex.ExerciseId,
                        Repetitions = ex.Repetitions,
                        DurationSeconds = ex.DurationSeconds,
                        Comment = string.IsNullOrWhiteSpace(ex.Comment) ? null : ex.Comment.Trim()
                    })
                    .ToList()
            })
            .ToList();

        dbContext.PatientTrainingPlanDays.AddRange(newDays);

        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetMappedPlanAsync(plan.Id, clinicId, cancellationToken);
    }

    public async Task<PatientTrainingPlanDayProgressResponseDto> CompleteDayAsync(
        Guid clinicId,
        Guid patientId,
        Guid planId,
        int dayNumber,
        CancellationToken cancellationToken = default)
    {
        if (dayNumber <= 0)
        {
            throw new InvalidOperationException("DayNumber must be greater than 0.");
        }

        await EnsurePatientPlanDayExistsAsync(clinicId, patientId, planId, dayNumber, cancellationToken);

        var plan = await dbContext.PatientTrainingPlans
            .FirstOrDefaultAsync(
                x => x.Id == planId && x.ClinicId == clinicId && x.PatientId == patientId && !x.IsDeleted,
                cancellationToken);

        if (plan is null)
        {
            throw new KeyNotFoundException("Training plan was not found.");
        }

        var existingProgress = await dbContext.PatientTrainingPlanDayProgresses
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.PatientId == patientId && x.PatientTrainingPlanId == planId && x.DayNumber == dayNumber,
                cancellationToken);

        await UpdatePlanStatusByCompletedDayAsync(plan, dayNumber, cancellationToken);

        if (existingProgress is not null)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return MapDayProgress(existingProgress);
        }

        var progress = new PatientTrainingPlanDayProgress
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            PatientTrainingPlanId = planId,
            DayNumber = dayNumber,
            StateRating = null,
            Notes = null,
            CompletedAtUtc = DateTime.UtcNow
        };

        dbContext.PatientTrainingPlanDayProgresses.Add(progress);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapDayProgress(progress);
    }

    public async Task<PatientTrainingPlanDayProgressResponseDto> UpdateDayProgressAsync(
        Guid clinicId,
        Guid patientId,
        Guid planId,
        int dayNumber,
        UpdatePatientTrainingPlanDayProgressRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (dayNumber <= 0)
        {
            throw new InvalidOperationException("DayNumber must be greater than 0.");
        }

        if (request.StateRating.HasValue && (request.StateRating.Value < 1 || request.StateRating.Value > 5))
        {
            throw new InvalidOperationException("StateRating must be between 1 and 5.");
        }

        if (request.StateRating is null && request.Notes is null)
        {
            throw new InvalidOperationException("At least one field (StateRating or Notes) must be provided.");
        }

        await EnsurePatientPlanDayExistsAsync(clinicId, patientId, planId, dayNumber, cancellationToken);

        var progress = await dbContext.PatientTrainingPlanDayProgresses
            .FirstOrDefaultAsync(
                x => x.PatientId == patientId && x.PatientTrainingPlanId == planId && x.DayNumber == dayNumber,
                cancellationToken);

        if (progress is null)
        {
            throw new InvalidOperationException("Day completion was not marked yet.");
        }

        if (request.StateRating.HasValue)
        {
            progress.StateRating = request.StateRating.Value;
        }

        if (request.Notes is not null)
        {
            progress.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return MapDayProgress(progress);
    }

    private async Task UpdatePlanStatusByCompletedDayAsync(
        PatientTrainingPlan plan,
        int completedDayNumber,
        CancellationToken cancellationToken)
    {
        var lastDayNumber = await dbContext.PatientTrainingPlanDays
            .AsNoTracking()
            .Where(x => x.PatientTrainingPlanId == plan.Id)
            .MaxAsync(x => (int?)x.DayNumber, cancellationToken);

        if (!lastDayNumber.HasValue)
        {
            return;
        }

        if (completedDayNumber >= lastDayNumber.Value)
        {
            plan.Status = PatientTrainingPlanStatus.Completed;
            return;
        }

        if (plan.Status == PatientTrainingPlanStatus.Assigned)
        {
            plan.Status = PatientTrainingPlanStatus.InProgress;
        }
    }

    private async Task ValidatePatientAsync(Guid clinicId, Guid patientId, CancellationToken cancellationToken)
    {
        var patient = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == patientId && x.ClinicId == clinicId && x.IsActive, cancellationToken);

        if (patient is null)
        {
            throw new KeyNotFoundException("Patient was not found in your clinic.");
        }

        if (patient.Role != UserRole.Patient)
        {
            throw new InvalidOperationException("Selected user must have Patient role.");
        }
    }

    private async Task EnsurePatientPlanDayExistsAsync(
        Guid clinicId,
        Guid patientId,
        Guid planId,
        int dayNumber,
        CancellationToken cancellationToken)
    {
        var planExists = await dbContext.PatientTrainingPlans
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == planId && x.ClinicId == clinicId && x.PatientId == patientId && !x.IsDeleted,
                cancellationToken);

        if (!planExists)
        {
            throw new KeyNotFoundException("Training plan was not found.");
        }

        var dayExists = await dbContext.PatientTrainingPlanDays
            .AsNoTracking()
            .AnyAsync(x => x.PatientTrainingPlanId == planId && x.DayNumber == dayNumber, cancellationToken);

        if (!dayExists)
        {
            throw new KeyNotFoundException("Training day was not found in this training plan.");
        }
    }

    private async Task ValidateDaysAsync(
        Guid clinicId,
        Guid currentUserId,
        bool isAdmin,
        List<CreatePatientTrainingPlanDayRequestDto> days,
        CancellationToken cancellationToken)
    {
        if (days.Count == 0)
        {
            throw new InvalidOperationException("Training plan must contain at least one day.");
        }

        if (days.Any(x => x.DayNumber <= 0))
        {
            throw new InvalidOperationException("DayNumber must be greater than 0.");
        }

        var duplicatedDays = days
            .GroupBy(x => x.DayNumber)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        if (duplicatedDays.Count > 0)
        {
            throw new InvalidOperationException("DayNumber values must be unique inside a training plan.");
        }

        var orderedDayNumbers = days
            .Select(x => x.DayNumber)
            .OrderBy(x => x)
            .ToList();

        if (orderedDayNumbers[0] != 1)
        {
            throw new InvalidOperationException("DayNumber sequence must start from 1.");
        }

        for (var index = 0; index < orderedDayNumbers.Count; index++)
        {
            var expected = index + 1;
            if (orderedDayNumbers[index] != expected)
            {
                throw new InvalidOperationException("DayNumber values must form a continuous sequence without gaps.");
            }
        }

        var allExerciseRequests = days.SelectMany(x => x.Exercises).ToList();

        foreach (var day in days)
        {
            if (day.IsRestDay && day.Exercises.Count > 0)
            {
                throw new InvalidOperationException("Rest day must not contain exercises.");
            }

            if (!day.IsRestDay && day.Exercises.Count == 0)
            {
                throw new InvalidOperationException("Non-rest day must contain at least one exercise.");
            }

            if (day.Exercises.Any(x => x.Order <= 0))
            {
                throw new InvalidOperationException("Exercise order must be greater than 0.");
            }

            var duplicatedOrder = day.Exercises
                .GroupBy(x => x.Order)
                .Any(g => g.Count() > 1);
            if (duplicatedOrder)
            {
                throw new InvalidOperationException("Exercise order must be unique inside a day.");
            }

            foreach (var ex in day.Exercises)
            {
                var hasRepetitions = ex.Repetitions.HasValue;
                var hasDuration = ex.DurationSeconds.HasValue;

                if (hasRepetitions == hasDuration)
                {
                    throw new InvalidOperationException("Each day exercise must have either repetitions or duration.");
                }

                if (ex.Repetitions is <= 0 || ex.DurationSeconds is <= 0)
                {
                    throw new InvalidOperationException("Repetitions and duration must be greater than 0 when provided.");
                }
            }
        }

        var requestedExerciseIds = allExerciseRequests
            .Select(x => x.ExerciseId)
            .Distinct()
            .ToList();

        var exercisesQuery = dbContext.Exercises
            .AsNoTracking()
            .Where(x => x.ClinicId == clinicId && !x.IsDeleted && requestedExerciseIds.Contains(x.Id));

        if (!isAdmin)
        {
            exercisesQuery = exercisesQuery.Where(x => x.UserId == currentUserId || x.UserId == null);
        }

        var existingExerciseIds = await exercisesQuery
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (existingExerciseIds.Count != requestedExerciseIds.Count)
        {
            throw new InvalidOperationException("One or more exercises were not found or not accessible in your clinic.");
        }

        var exercisesById = await dbContext.Exercises
            .AsNoTracking()
            .Where(x => existingExerciseIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        foreach (var ex in allExerciseRequests)
        {
            var exerciseType = exercisesById[ex.ExerciseId].Type;
            if (exerciseType == ExerciseType.Repetition && ex.Repetitions is null)
            {
                throw new InvalidOperationException("Repetition exercise must define repetitions.");
            }

            if (exerciseType == ExerciseType.Time && ex.DurationSeconds is null)
            {
                throw new InvalidOperationException("Time exercise must define duration.");
            }
        }
    }

    private async Task ValidateDoctorAssignmentIfRequiredAsync(
        Guid clinicId,
        Guid currentUserId,
        Guid patientId,
        bool isAdmin,
        CancellationToken cancellationToken)
    {
        if (isAdmin)
        {
            return;
        }

        var assignmentExists = await dbContext.DoctorPatientAssignments
            .AsNoTracking()
            .AnyAsync(
                x => x.ClinicId == clinicId && x.DoctorId == currentUserId && x.PatientId == patientId,
                cancellationToken);

        if (!assignmentExists)
        {
            throw new UnauthorizedAccessException("Doctor can create or edit plans only for assigned patients.");
        }
    }

    private async Task<PatientTrainingPlanResponseDto> GetMappedPlanAsync(Guid planId, Guid clinicId, CancellationToken cancellationToken)
    {
        var plan = await dbContext.PatientTrainingPlans
            .AsNoTracking()
            .Include(x => x.Days)
            .ThenInclude(x => x.Exercises)
            .ThenInclude(x => x.Exercise)
            .FirstOrDefaultAsync(x => x.Id == planId && x.ClinicId == clinicId, cancellationToken);

        if (plan is null)
        {
            throw new KeyNotFoundException("Training plan was not found.");
        }

        return new PatientTrainingPlanResponseDto
        {
            Id = plan.Id,
            ClinicId = plan.ClinicId,
            PatientId = plan.PatientId,
            CreatedByUserId = plan.CreatedByUserId,
            Name = plan.Name,
            Description = plan.Description,
            StartDate = plan.StartDate,
            Status = plan.Status,
            IsDeleted = plan.IsDeleted,
            Days = plan.Days
                .OrderBy(x => x.DayNumber)
                .Select(day => new PatientTrainingPlanDayResponseDto
                {
                    Id = day.Id,
                    DayNumber = day.DayNumber,
                    IsRestDay = day.IsRestDay,
                    Notes = day.Notes,
                    Exercises = day.Exercises
                        .OrderBy(x => x.Order)
                        .Select(ex => new PatientTrainingPlanDayExerciseResponseDto
                        {
                            Id = ex.Id,
                            Order = ex.Order,
                            ExerciseId = ex.ExerciseId,
                            Exercise = new ExerciseResponseDto
                            {
                                Id = ex.Exercise.Id,
                                ClinicId = ex.Exercise.ClinicId,
                                UserId = ex.Exercise.UserId,
                                Name = ex.Exercise.Name,
                                Description = ex.Exercise.Description,
                                MediaUrls = ex.Exercise.MediaUrls.ToList(),
                                IsDeleted = ex.Exercise.IsDeleted,
                                Type = ex.Exercise.Type,
                                Steps = ex.Exercise.Steps.ToList()
                            },
                            Repetitions = ex.Repetitions,
                            DurationSeconds = ex.DurationSeconds,
                            Comment = ex.Comment
                        })
                        .ToList()
                })
                .ToList()
        };
    }

    private static PatientTrainingPlanDayProgressResponseDto MapDayProgress(PatientTrainingPlanDayProgress progress)
    {
        return new PatientTrainingPlanDayProgressResponseDto
        {
            Id = progress.Id,
            PatientId = progress.PatientId,
            PatientTrainingPlanId = progress.PatientTrainingPlanId,
            DayNumber = progress.DayNumber,
            StateRating = progress.StateRating,
            Notes = progress.Notes,
            CompletedAtUtc = progress.CompletedAtUtc
        };
    }
}

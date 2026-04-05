using MedReability.Application.DTOs.Assignments;
using MedReability.Application.DTOs.Common;
using MedReability.Application.DTOs.Exercises;
using MedReability.Application.Interfaces.Security;
using MedReability.Application.Interfaces.Services;
using MedReability.Domain.Entities;
using MedReability.Domain.Enums;
using MedReability.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MedReability.Infrastructure.Services;

public class DoctorPatientAssignmentService(
    AppDbContext dbContext,
    IAccessPolicyService accessPolicyService) : IDoctorPatientAssignmentService
{
    public async Task<DoctorPatientAssignmentResponseDto> AssignAsync(
        Guid clinicId,
        Guid doctorId,
        Guid patientId,
        CancellationToken cancellationToken = default)
    {
        await ValidateDoctorAndPatientAsync(clinicId, doctorId, patientId, cancellationToken);

        var exists = await dbContext.DoctorPatientAssignments
            .AnyAsync(
                x => x.ClinicId == clinicId && x.DoctorId == doctorId && x.PatientId == patientId,
                cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("This doctor-patient assignment already exists.");
        }

        var assignment = new DoctorPatientAssignment
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            DoctorId = doctorId,
            PatientId = patientId
        };

        dbContext.DoctorPatientAssignments.Add(assignment);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(assignment);
    }

    public async Task<List<DoctorPatientListItemDto>> GetDoctorPatientsAsync(
        Guid clinicId,
        Guid doctorId,
        CancellationToken cancellationToken = default)
    {
        var doctor = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == doctorId && x.IsActive,
                cancellationToken);

        if (doctor is null || !accessPolicyService.IsSameClinic(clinicId, doctor.ClinicId))
        {
            throw new KeyNotFoundException("Doctor was not found in your clinic.");
        }

        if (doctor.Role != UserRole.Doctor)
        {
            throw new InvalidOperationException("Current user must have Doctor role.");
        }

        return await dbContext.DoctorPatientAssignments
            .AsNoTracking()
            .Where(x => x.ClinicId == clinicId && x.DoctorId == doctorId)
            .Join(
                dbContext.Users.AsNoTracking(),
                assignment => assignment.PatientId,
                patient => patient.Id,
                (assignment, patient) => new DoctorPatientListItemDto
                {
                    AssignmentId = assignment.Id,
                    PatientId = patient.Id,
                    FirstName = patient.FirstName,
                    Patronymic = patient.Patronymic,
                    LastName = patient.LastName,
                    Email = patient.Email,
                    PhoneNumber = patient.PhoneNumber,
                    IsActive = patient.IsActive,
                    HasPlan = dbContext.PatientTrainingPlans.Any(plan =>
                        plan.ClinicId == clinicId &&
                        plan.PatientId == patient.Id &&
                        !plan.IsDeleted)
                })
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<DoctorPatientOverviewResponseDto> GetPatientOverviewAsync(
        Guid clinicId,
        Guid doctorId,
        Guid patientId,
        DateOnly? startDate,
        CancellationToken cancellationToken = default)
    {
        var doctor = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == doctorId && x.IsActive,
                cancellationToken);

        if (doctor is null || !accessPolicyService.IsSameClinic(clinicId, doctor.ClinicId))
        {
            throw new KeyNotFoundException("Doctor was not found in your clinic.");
        }

        if (doctor.Role != UserRole.Doctor)
        {
            throw new InvalidOperationException("Current user must have Doctor role.");
        }

        var assignmentExists = await dbContext.DoctorPatientAssignments
            .AsNoTracking()
            .AnyAsync(
                x => x.ClinicId == clinicId && x.DoctorId == doctorId && x.PatientId == patientId,
                cancellationToken);

        if (!assignmentExists)
        {
            throw new KeyNotFoundException("Patient was not found in your assignment list.");
        }

        var patient = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == patientId && x.ClinicId == clinicId, cancellationToken);

        if (patient is null)
        {
            throw new KeyNotFoundException("Patient was not found in your clinic.");
        }

        if (patient.Role != UserRole.Patient)
        {
            throw new InvalidOperationException("Selected user must have Patient role.");
        }

        var patientDto = new DoctorPatientOverviewPatientDto
        {
            Id = patient.Id,
            ClinicId = patient.ClinicId,
            FirstName = patient.FirstName,
            Patronymic = patient.Patronymic,
            LastName = patient.LastName,
            Email = patient.Email,
            PhoneNumber = patient.PhoneNumber,
            ImageUrl = patient.ImageUrl,
            IsActive = patient.IsActive
        };

        var weekStart = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var latestPlan = await dbContext.PatientTrainingPlans
            .AsNoTracking()
            .Where(x => x.ClinicId == clinicId && x.PatientId == patientId && !x.IsDeleted)
            .OrderByDescending(x => x.StartDate)
            .ThenByDescending(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (latestPlan is null)
        {
            return new DoctorPatientOverviewResponseDto
            {
                Patient = patientDto,
                HasPlan = false,
                Plan = null,
                Progress = null,
                Days = [],
                TodayWorkout = null
            };
        }

        var plan = await dbContext.PatientTrainingPlans
            .AsNoTracking()
            .Include(x => x.Days)
            .ThenInclude(x => x.Exercises)
            .ThenInclude(x => x.Exercise)
            .FirstAsync(x => x.Id == latestPlan.Id, cancellationToken);

        var progresses = await dbContext.PatientTrainingPlanDayProgresses
            .AsNoTracking()
            .Where(x => x.PatientId == patientId && x.PatientTrainingPlanId == plan.Id)
            .ToListAsync(cancellationToken);

        var daysByNumber = plan.Days.ToDictionary(x => x.DayNumber, x => x);
        var progressByDayNumber = progresses.ToDictionary(x => x.DayNumber, x => x);

        var days = new List<DoctorPatientOverviewDayDto>(7);
        for (var index = 0; index < 7; index++)
        {
            var date = weekStart.AddDays(index);
            var dayNumber = GetDayNumberFromPlanStart(plan.StartDate, date);

            if (dayNumber <= 0 || !daysByNumber.TryGetValue(dayNumber, out var planDay))
            {
                days.Add(new DoctorPatientOverviewDayDto
                {
                    Date = date,
                    DayNumber = null,
                    DayType = DoctorPatientOverviewDayType.Empty,
                    HasTraining = false,
                    IsCompleted = false,
                    StateRating = null,
                    Notes = null
                });
                continue;
            }

            progressByDayNumber.TryGetValue(dayNumber, out var progress);
            var isCompleted = progress is not null;

            days.Add(new DoctorPatientOverviewDayDto
            {
                Date = date,
                DayNumber = dayNumber,
                DayType = planDay.IsRestDay ? DoctorPatientOverviewDayType.Rest : DoctorPatientOverviewDayType.Training,
                HasTraining = !planDay.IsRestDay,
                IsCompleted = isCompleted,
                StateRating = progress?.StateRating,
                Notes = progress?.Notes
            });
        }

        var plannedTrainingDaysCount = days.Count(x => x.HasTraining);
        var completedDaysCount = days.Count(x => x.HasTraining && x.IsCompleted);
        var completionPercent = plannedTrainingDaysCount == 0
            ? 0
            : (int)Math.Round((double)completedDaysCount * 100 / plannedTrainingDaysCount, MidpointRounding.AwayFromZero);

        DoctorPatientTodayWorkoutDto? todayWorkout = null;
        var todayDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var todayDayNumber = GetDayNumberFromPlanStart(plan.StartDate, todayDate);
        if (todayDayNumber > 0 &&
            daysByNumber.TryGetValue(todayDayNumber, out var todayPlanDay) &&
            !todayPlanDay.IsRestDay)
        {
            var todayExercises = todayPlanDay.Exercises
                .OrderBy(x => x.Order)
                .Select(x => new DoctorPatientTodayWorkoutExerciseDto
                {
                    Order = x.Order,
                    Exercise = new ExerciseResponseDto
                    {
                        Id = x.Exercise.Id,
                        ClinicId = x.Exercise.ClinicId,
                        UserId = x.Exercise.UserId,
                        Name = x.Exercise.Name,
                        Description = x.Exercise.Description,
                        MediaUrls = x.Exercise.MediaUrls.ToList(),
                        IsDeleted = x.Exercise.IsDeleted,
                        Type = x.Exercise.Type,
                        Steps = x.Exercise.Steps.ToList()
                    },
                    Repetitions = x.Repetitions,
                    DurationSeconds = x.DurationSeconds,
                    Comment = x.Comment
                })
                .ToList();

            todayWorkout = new DoctorPatientTodayWorkoutDto
            {
                Date = todayDate,
                DayNumber = todayDayNumber,
                IsCompletedToday = progressByDayNumber.ContainsKey(todayDayNumber),
                IsRestDay = false,
                Exercises = todayExercises
            };
        }

        return new DoctorPatientOverviewResponseDto
        {
            Patient = patientDto,
            HasPlan = true,
            Plan = new DoctorPatientOverviewPlanDto
            {
                Id = plan.Id,
                Name = plan.Name,
                Status = plan.Status,
                StartDate = plan.StartDate
            },
            Progress = new DoctorPatientOverviewProgressDto
            {
                CompletedDaysCount = completedDaysCount,
                PlannedTrainingDaysCount = plannedTrainingDaysCount,
                CompletionPercent = completionPercent
            },
            Days = days,
            TodayWorkout = todayWorkout
        };
    }

    public async Task<PagedResultDto<DoctorPatientAssignmentListItemDto>> GetAssignmentsAsync(
        Guid clinicId,
        DoctorPatientAssignmentsQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize switch
        {
            < 1 => 20,
            > 100 => 100,
            _ => query.PageSize
        };

        var assignmentsQuery = dbContext.DoctorPatientAssignments
            .AsNoTracking()
            .Where(x => x.ClinicId == clinicId);

        if (query.DoctorId.HasValue)
        {
            assignmentsQuery = assignmentsQuery.Where(x => x.DoctorId == query.DoctorId.Value);
        }

        if (query.PatientId.HasValue)
        {
            assignmentsQuery = assignmentsQuery.Where(x => x.PatientId == query.PatientId.Value);
        }

        var projectedQuery = assignmentsQuery
            .Join(
                dbContext.Users.AsNoTracking(),
                assignment => assignment.DoctorId,
                doctor => doctor.Id,
                (assignment, doctor) => new { assignment, doctor })
            .Join(
                dbContext.Users.AsNoTracking(),
                left => left.assignment.PatientId,
                patient => patient.Id,
                (left, patient) => new DoctorPatientAssignmentListItemDto
                {
                    AssignmentId = left.assignment.Id,
                    Doctor = new AssignmentUserDto
                    {
                        Id = left.doctor.Id,
                        FirstName = left.doctor.FirstName,
                        Patronymic = left.doctor.Patronymic,
                        LastName = left.doctor.LastName,
                        Email = left.doctor.Email,
                        PhoneNumber = left.doctor.PhoneNumber,
                        IsActive = left.doctor.IsActive
                    },
                    Patient = new AssignmentUserDto
                    {
                        Id = patient.Id,
                        FirstName = patient.FirstName,
                        Patronymic = patient.Patronymic,
                        LastName = patient.LastName,
                        Email = patient.Email,
                        PhoneNumber = patient.PhoneNumber,
                        IsActive = patient.IsActive
                    }
                });

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLowerInvariant();
            projectedQuery = projectedQuery.Where(x =>
                x.Doctor.FirstName.ToLower().Contains(search) ||
                x.Doctor.LastName.ToLower().Contains(search) ||
                x.Doctor.Patronymic.ToLower().Contains(search) ||
                x.Patient.FirstName.ToLower().Contains(search) ||
                x.Patient.LastName.ToLower().Contains(search) ||
                x.Patient.Patronymic.ToLower().Contains(search));
        }

        var totalCount = await projectedQuery.CountAsync(cancellationToken);

        var items = await projectedQuery
            .OrderBy(x => x.Patient.LastName)
            .ThenBy(x => x.Patient.FirstName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<DoctorPatientAssignmentListItemDto>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items
        };
    }

    public async Task<bool> DeleteAsync(
        Guid clinicId,
        Guid assignmentId,
        CancellationToken cancellationToken = default)
    {
        var assignment = await dbContext.DoctorPatientAssignments
            .FirstOrDefaultAsync(x => x.Id == assignmentId, cancellationToken);

        if (assignment is null || !accessPolicyService.IsSameClinic(clinicId, assignment.ClinicId))
        {
            return false;
        }

        dbContext.DoctorPatientAssignments.Remove(assignment);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task ValidateDoctorAndPatientAsync(
        Guid clinicId,
        Guid doctorId,
        Guid patientId,
        CancellationToken cancellationToken)
    {
        if (doctorId == Guid.Empty || patientId == Guid.Empty)
        {
            throw new InvalidOperationException("DoctorId and PatientId are required.");
        }

        if (doctorId == patientId)
        {
            throw new InvalidOperationException("Doctor and patient cannot be the same user.");
        }

        var doctor = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == doctorId && x.IsActive,
                cancellationToken);

        if (doctor is null || !accessPolicyService.IsSameClinic(clinicId, doctor.ClinicId))
        {
            throw new KeyNotFoundException("Doctor was not found in your clinic.");
        }

        if (doctor.Role != UserRole.Doctor)
        {
            throw new InvalidOperationException("Selected doctor user must have Doctor role.");
        }

        var patient = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == patientId && x.IsActive,
                cancellationToken);

        if (patient is null || !accessPolicyService.IsSameClinic(clinicId, patient.ClinicId))
        {
            throw new KeyNotFoundException("Patient was not found in your clinic.");
        }

        if (patient.Role != UserRole.Patient)
        {
            throw new InvalidOperationException("Selected patient user must have Patient role.");
        }
    }

    private static DoctorPatientAssignmentResponseDto Map(DoctorPatientAssignment assignment)
    {
        return new DoctorPatientAssignmentResponseDto
        {
            Id = assignment.Id,
            ClinicId = assignment.ClinicId,
            DoctorId = assignment.DoctorId,
            PatientId = assignment.PatientId
        };
    }

    private static int GetDayNumberFromPlanStart(DateOnly startDate, DateOnly date)
    {
        var difference = date.ToDateTime(TimeOnly.MinValue) - startDate.ToDateTime(TimeOnly.MinValue);
        return difference.Days + 1;
    }
}

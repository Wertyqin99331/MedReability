using MedReability.Application.DTOs.TrainingPlans;
using MedReability.Application.Interfaces.Security;
using MedReability.Domain.Entities;
using MedReability.Domain.Enums;
using MedReability.Infrastructure.Persistence;
using MedReability.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace MedReability.Api.Tests.Progress;

public class PatientTrainingPlanProgressServiceTests
{
    [Fact]
    public async Task CompleteDay_FirstDay_SetsPlanInProgress_AndCreatesProgress()
    {
        await using var db = CreateDbContext();
        var data = await SeedAsync(db);
        var service = CreateService(db);

        var result = await service.CompleteDayAsync(data.ClinicAId, data.PatientAId, data.PlanId, dayNumber: 1);

        var plan = await db.PatientTrainingPlans.SingleAsync(x => x.Id == data.PlanId);
        var progressRows = await db.PatientTrainingPlanDayProgresses
            .Where(x => x.PatientTrainingPlanId == data.PlanId && x.DayNumber == 1)
            .ToListAsync();

        Assert.Equal(PatientTrainingPlanStatus.InProgress, plan.Status);
        Assert.Single(progressRows);
        Assert.Equal(data.PatientAId, result.PatientId);
    }

    [Fact]
    public async Task CompleteDay_LastDay_SetsPlanCompleted()
    {
        await using var db = CreateDbContext();
        var data = await SeedAsync(db);
        var service = CreateService(db);

        await service.CompleteDayAsync(data.ClinicAId, data.PatientAId, data.PlanId, dayNumber: 2);

        var plan = await db.PatientTrainingPlans.SingleAsync(x => x.Id == data.PlanId);
        Assert.Equal(PatientTrainingPlanStatus.Completed, plan.Status);
    }

    [Fact]
    public async Task CompleteDay_RepeatedCall_DoesNotCreateDuplicateProgress()
    {
        await using var db = CreateDbContext();
        var data = await SeedAsync(db);
        var service = CreateService(db);

        await service.CompleteDayAsync(data.ClinicAId, data.PatientAId, data.PlanId, dayNumber: 1);
        await service.CompleteDayAsync(data.ClinicAId, data.PatientAId, data.PlanId, dayNumber: 1);

        var count = await db.PatientTrainingPlanDayProgresses
            .CountAsync(x => x.PatientTrainingPlanId == data.PlanId && x.DayNumber == 1);

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task UpdateDayProgress_WithoutCompletion_ThrowsInvalidOperation()
    {
        await using var db = CreateDbContext();
        var data = await SeedAsync(db);
        var service = CreateService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateDayProgressAsync(
                data.ClinicAId,
                data.PatientAId,
                data.PlanId,
                dayNumber: 1,
                new UpdatePatientTrainingPlanDayProgressRequestDto
                {
                    StateRating = 4
                }));
    }

    [Fact]
    public async Task UpdateDayProgress_UpdatesRatingAndNotes()
    {
        await using var db = CreateDbContext();
        var data = await SeedAsync(db);
        var service = CreateService(db);

        await service.CompleteDayAsync(data.ClinicAId, data.PatientAId, data.PlanId, dayNumber: 1);
        var updated = await service.UpdateDayProgressAsync(
            data.ClinicAId,
            data.PatientAId,
            data.PlanId,
            dayNumber: 1,
            new UpdatePatientTrainingPlanDayProgressRequestDto
            {
                StateRating = 5,
                Notes = "Feeling better"
            });

        Assert.Equal(5, updated.StateRating);
        Assert.Equal("Feeling better", updated.Notes);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"progress-service-tests-{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }

    private static PatientTrainingPlanService CreateService(AppDbContext dbContext)
    {
        IAccessPolicyService accessPolicyService = new AccessPolicyService();
        return new PatientTrainingPlanService(dbContext, accessPolicyService);
    }

    private static async Task<SeedData> SeedAsync(AppDbContext db)
    {
        var clinicA = new ClinicEntity { Id = Guid.NewGuid(), Name = "ClinicEntity A" };
        var doctorA = CreateUser(clinicA.Id, UserRole.Doctor, "doctor.a@clinic.local");
        var patientA = CreateUser(clinicA.Id, UserRole.Patient, "patient.a@clinic.local");

        var repetitionExercise = CreateExercise(clinicA.Id, doctorA.Id, ExerciseType.Repetition, "Repetition exercise");
        var timeExercise = CreateExercise(clinicA.Id, null, ExerciseType.Time, "Time exercise");

        var plan = new PatientTrainingPlanEntity
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicA.Id,
            PatientId = patientA.Id,
            CreatedByUserId = doctorA.Id,
            Name = "Plan for progress",
            StartDate = new DateOnly(2026, 4, 6),
            Status = PatientTrainingPlanStatus.Assigned,
            IsDeleted = false,
            Days =
            [
                new PatientTrainingPlanDayEntity
                {
                    Id = Guid.NewGuid(),
                    DayNumber = 1,
                    IsRestDay = false,
                    Exercises =
                    [
                        new PatientTrainingPlanDayExerciseEntity
                        {
                            Id = Guid.NewGuid(),
                            Order = 1,
                            ExerciseId = repetitionExercise.Id,
                            Repetitions = 12
                        }
                    ]
                },
                new PatientTrainingPlanDayEntity
                {
                    Id = Guid.NewGuid(),
                    DayNumber = 2,
                    IsRestDay = false,
                    Exercises =
                    [
                        new PatientTrainingPlanDayExerciseEntity
                        {
                            Id = Guid.NewGuid(),
                            Order = 1,
                            ExerciseId = timeExercise.Id,
                            DurationSeconds = 45
                        }
                    ]
                }
            ]
        };

        db.Clinics.Add(clinicA);
        db.Users.AddRange(doctorA, patientA);
        db.Exercises.AddRange(repetitionExercise, timeExercise);
        db.PatientTrainingPlans.Add(plan);

        await db.SaveChangesAsync();

        return new SeedData(clinicA.Id, patientA.Id, plan.Id);
    }

    private static UserEntity CreateUser(Guid clinicId, UserRole role, string email)
    {
        return new UserEntity
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            Email = email,
            PasswordHash = "hash",
            FirstName = "First",
            Patronymic = "Middle",
            LastName = "Last",
            PhoneNumber = "+79000000000",
            Role = role,
            IsActive = true
        };
    }

    private static ExerciseEntity CreateExercise(Guid clinicId, Guid? userId, ExerciseType type, string name)
    {
        return new ExerciseEntity
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            UserId = userId,
            Name = name,
            Description = "Description",
            MediaUrls = [],
            IsDeleted = false,
            Type = type,
            Steps = ["Step 1"]
        };
    }

    private sealed record SeedData(
        Guid ClinicAId,
        Guid PatientAId,
        Guid PlanId);
}


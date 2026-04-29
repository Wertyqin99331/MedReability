using MedReability.Application.DTOs.TrainingPlans;
using MedReability.Application.Interfaces.Security;
using MedReability.Domain.Entities;
using MedReability.Domain.Enums;
using MedReability.Infrastructure.Persistence;
using MedReability.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace MedReability.Api.Tests.Programs;

public class PatientTrainingPlanServiceTests
{
    [Fact]
    public async Task Create_DoctorWithoutAssignment_ThrowsUnauthorized()
    {
        await using var db = CreateDbContext();
        var data = await SeedAsync(db);
        var service = CreateService(db);

        var request = BuildValidPlanRequest(data.PatientAId, data.OwnExerciseId, data.GlobalExerciseId);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.CreateAsync(data.ClinicAId, data.DoctorAId, isAdmin: false, request));
    }

    [Fact]
    public async Task Create_DoctorWithAssignment_CreatesPlanWithAssignedStatus()
    {
        await using var db = CreateDbContext();
        var data = await SeedAsync(db);
        var service = CreateService(db);

        db.DoctorPatientAssignments.Add(new DoctorPatientAssignmentEntity
        {
            Id = Guid.NewGuid(),
            ClinicId = data.ClinicAId,
            DoctorId = data.DoctorAId,
            PatientId = data.PatientAId
        });
        await db.SaveChangesAsync();

        var request = BuildValidPlanRequest(data.PatientAId, data.OwnExerciseId, data.GlobalExerciseId);
        var result = await service.CreateAsync(data.ClinicAId, data.DoctorAId, isAdmin: false, request);

        Assert.Equal(PatientTrainingPlanStatus.Assigned, result.Status);
        Assert.Equal(2, result.Days.Count);
        Assert.Equal(2, result.Days.Single(x => x.DayNumber == 1).Exercises.Count);
    }

    [Fact]
    public async Task Create_WithGapInDays_ThrowsInvalidOperation()
    {
        await using var db = CreateDbContext();
        var data = await SeedAsync(db);
        var service = CreateService(db);

        var request = BuildValidPlanRequest(data.PatientAId, data.OwnExerciseId, data.GlobalExerciseId);
        request.Days =
        [
            request.Days[0],
            new CreatePatientTrainingPlanDayRequestDto
            {
                DayNumber = 3,
                IsRestDay = true
            }
        ];

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(data.ClinicAId, data.AdminAId, isAdmin: true, request));
    }

    [Fact]
    public async Task Create_WithRestBetweenSetsAndSetsLessThanTwo_ThrowsInvalidOperation()
    {
        await using var db = CreateDbContext();
        var data = await SeedAsync(db);
        var service = CreateService(db);

        var request = BuildValidPlanRequest(data.PatientAId, data.OwnExerciseId, data.GlobalExerciseId);
        request.Days[0].Exercises[0].Sets = 1;
        request.Days[0].Exercises[0].RestBetweenSets = 30;

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(data.ClinicAId, data.AdminAId, isAdmin: true, request));
    }

    [Fact]
    public async Task Create_WithSetsAndRestBetweenSets_PersistsFields()
    {
        await using var db = CreateDbContext();
        var data = await SeedAsync(db);
        var service = CreateService(db);

        db.DoctorPatientAssignments.Add(new DoctorPatientAssignmentEntity
        {
            Id = Guid.NewGuid(),
            ClinicId = data.ClinicAId,
            DoctorId = data.DoctorAId,
            PatientId = data.PatientAId
        });
        await db.SaveChangesAsync();

        var request = BuildValidPlanRequest(data.PatientAId, data.OwnExerciseId, data.GlobalExerciseId);
        request.Days[0].Exercises[0].Sets = 2;
        request.Days[0].Exercises[0].RestBetweenSets = 30;

        var result = await service.CreateAsync(data.ClinicAId, data.DoctorAId, isAdmin: false, request);

        var exercise = result.Days
            .Single(x => x.DayNumber == 1)
            .Exercises
            .Single(x => x.Order == 1);

        Assert.Equal(2, exercise.Sets);
        Assert.Equal(30, exercise.RestBetweenSets);
    }

    [Fact]
    public async Task Update_NonOwnerDoctor_ThrowsUnauthorized()
    {
        await using var db = CreateDbContext();
        var data = await SeedAsync(db);
        var service = CreateService(db);

        db.DoctorPatientAssignments.Add(new DoctorPatientAssignmentEntity
        {
            Id = Guid.NewGuid(),
            ClinicId = data.ClinicAId,
            DoctorId = data.DoctorAId,
            PatientId = data.PatientAId
        });
        await db.SaveChangesAsync();

        var created = await service.CreateAsync(
            data.ClinicAId,
            data.DoctorAId,
            isAdmin: false,
            BuildValidPlanRequest(data.PatientAId, data.OwnExerciseId, data.GlobalExerciseId));

        var update = new UpdatePatientTrainingPlanRequestDto
        {
            Name = "Updated by other doctor",
            Description = "Forbidden update",
            StartDate = created.StartDate.AddDays(1),
            Days = BuildValidPlanRequest(data.PatientAId, data.OwnExerciseId, data.GlobalExerciseId).Days
        };

        db.ChangeTracker.Clear();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.UpdateAsync(data.ClinicAId, data.DoctorA2Id, isAdmin: false, created.Id, update));
    }

    [Fact]
    public async Task Update_Admin_CanEditForeignPlan()
    {
        await using var db = CreateDbContext();
        var data = await SeedAsync(db);
        var service = CreateService(db);

        db.DoctorPatientAssignments.Add(new DoctorPatientAssignmentEntity
        {
            Id = Guid.NewGuid(),
            ClinicId = data.ClinicAId,
            DoctorId = data.DoctorAId,
            PatientId = data.PatientAId
        });
        await db.SaveChangesAsync();

        var created = await service.CreateAsync(
            data.ClinicAId,
            data.DoctorAId,
            isAdmin: false,
            BuildValidPlanRequest(data.PatientAId, data.OwnExerciseId, data.GlobalExerciseId));

        var update = new UpdatePatientTrainingPlanRequestDto
        {
            Name = "Admin updated",
            Description = "Updated by admin",
            StartDate = created.StartDate.AddDays(2),
            Days = BuildValidPlanRequest(data.PatientAId, data.OwnExerciseId, data.GlobalExerciseId).Days
        };

        db.ChangeTracker.Clear();

        var updated = await service.UpdateAsync(data.ClinicAId, data.AdminAId, isAdmin: true, created.Id, update);

        Assert.Equal("Admin updated", updated.Name);
        Assert.Equal(created.Id, updated.Id);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"program-service-tests-{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }

    private static PatientTrainingPlanService CreateService(AppDbContext dbContext)
    {
        IAccessPolicyService accessPolicyService = new AccessPolicyService();
        return new PatientTrainingPlanService(dbContext, accessPolicyService);
    }

    private static CreatePatientTrainingPlanRequestDto BuildValidPlanRequest(Guid patientId, Guid ownExerciseId, Guid globalExerciseId)
    {
        return new CreatePatientTrainingPlanRequestDto
        {
            PatientId = patientId,
            Name = "Recovery Plan",
            Description = "Plan description",
            StartDate = new DateOnly(2026, 4, 6),
            Days =
            [
                new CreatePatientTrainingPlanDayRequestDto
                {
                    DayNumber = 1,
                    IsRestDay = false,
                    Notes = "Main training day",
                    Exercises =
                    [
                        new CreatePatientTrainingPlanDayExerciseRequestDto
                        {
                            ExerciseId = ownExerciseId,
                            Order = 1,
                            Repetitions = 10
                        },
                        new CreatePatientTrainingPlanDayExerciseRequestDto
                        {
                            ExerciseId = globalExerciseId,
                            Order = 2,
                            DurationSeconds = 60
                        }
                    ]
                },
                new CreatePatientTrainingPlanDayRequestDto
                {
                    DayNumber = 2,
                    IsRestDay = true,
                    Notes = "Rest day"
                }
            ]
        };
    }

    private static async Task<SeedData> SeedAsync(AppDbContext db)
    {
        var clinicA = new ClinicEntity { Id = Guid.NewGuid(), Name = "ClinicEntity A" };
        var clinicB = new ClinicEntity { Id = Guid.NewGuid(), Name = "ClinicEntity B" };

        var adminA = CreateUser(clinicA.Id, UserRole.Admin, "admin.a@clinic.local");
        var doctorA = CreateUser(clinicA.Id, UserRole.Doctor, "doctor.a@clinic.local");
        var doctorA2 = CreateUser(clinicA.Id, UserRole.Doctor, "doctor.a2@clinic.local");
        var patientA = CreateUser(clinicA.Id, UserRole.Patient, "patient.a@clinic.local");
        var doctorB = CreateUser(clinicB.Id, UserRole.Doctor, "doctor.b@clinic.local");

        var ownExercise = CreateExercise(clinicA.Id, doctorA.Id, ExerciseType.Repetition, "Own repetition");
        var globalExercise = CreateExercise(clinicA.Id, null, ExerciseType.Time, "Global time");
        var clinicBExercise = CreateExercise(clinicB.Id, doctorB.Id, ExerciseType.Repetition, "Foreign exercise");

        db.Clinics.AddRange(clinicA, clinicB);
        db.Users.AddRange(adminA, doctorA, doctorA2, patientA, doctorB);
        db.Exercises.AddRange(ownExercise, globalExercise, clinicBExercise);
        await db.SaveChangesAsync();

        return new SeedData(
            clinicA.Id,
            adminA.Id,
            doctorA.Id,
            doctorA2.Id,
            patientA.Id,
            ownExercise.Id,
            globalExercise.Id);
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
        Guid AdminAId,
        Guid DoctorAId,
        Guid DoctorA2Id,
        Guid PatientAId,
        Guid OwnExerciseId,
        Guid GlobalExerciseId);
}

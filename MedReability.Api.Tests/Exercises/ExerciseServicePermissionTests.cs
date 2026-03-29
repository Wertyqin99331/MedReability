using MedReability.Application.DTOs.Exercises;
using MedReability.Application.Interfaces.Storage;
using MedReability.Domain.Entities;
using MedReability.Domain.Enums;
using MedReability.Infrastructure.Persistence;
using MedReability.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace MedReability.Api.Tests.Exercises;

public class ExerciseServicePermissionTests
{
    [Fact]
    public async Task GetExercises_AllFalse_Returns_Only_Own()
    {
        await using var db = CreateDbContext();
        var data = await SeedAsync(db);
        var service = new ExerciseService(db, new TestMediaStorageService());

        var result = await service.GetExercisesAsync(
            data.ClinicAId,
            data.DoctorAId,
            new ListExercisesQueryDto { PageNumber = 1, PageSize = 50, All = false });

        var ids = result.Items.Select(x => x.Id).ToHashSet();
        Assert.Single(ids);
        Assert.Contains(data.OwnExerciseId, ids);
        Assert.DoesNotContain(data.GlobalExerciseId, ids);
        Assert.DoesNotContain(data.OtherDoctorExerciseId, ids);
    }

    [Fact]
    public async Task GetExercises_AllTrue_Returns_Own_And_Global()
    {
        await using var db = CreateDbContext();
        var data = await SeedAsync(db);
        var service = new ExerciseService(db, new TestMediaStorageService());

        var result = await service.GetExercisesAsync(
            data.ClinicAId,
            data.DoctorAId,
            new ListExercisesQueryDto { PageNumber = 1, PageSize = 50, All = true });

        var ids = result.Items.Select(x => x.Id).ToHashSet();
        Assert.Equal(2, ids.Count);
        Assert.Contains(data.OwnExerciseId, ids);
        Assert.Contains(data.GlobalExerciseId, ids);
        Assert.DoesNotContain(data.OtherDoctorExerciseId, ids);
    }

    [Fact]
    public async Task GetExerciseById_Doctor_Can_Get_Own_And_Global_Only()
    {
        await using var db = CreateDbContext();
        var data = await SeedAsync(db);
        var service = new ExerciseService(db, new TestMediaStorageService());

        var own = await service.GetExerciseByIdAsync(data.ClinicAId, data.DoctorAId, data.OwnExerciseId, isAdmin: false);
        Assert.Equal(data.OwnExerciseId, own.Id);

        var global = await service.GetExerciseByIdAsync(data.ClinicAId, data.DoctorAId, data.GlobalExerciseId, isAdmin: false);
        Assert.Equal(data.GlobalExerciseId, global.Id);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.GetExerciseByIdAsync(data.ClinicAId, data.DoctorAId, data.OtherDoctorExerciseId, isAdmin: false));
    }

    [Fact]
    public async Task GetExerciseById_Admin_Can_Get_Any_In_Clinic()
    {
        await using var db = CreateDbContext();
        var data = await SeedAsync(db);
        var service = new ExerciseService(db, new TestMediaStorageService());

        var result = await service.GetExerciseByIdAsync(data.ClinicAId, data.AdminAId, data.OtherDoctorExerciseId, isAdmin: true);

        Assert.Equal(data.OtherDoctorExerciseId, result.Id);
    }

    [Fact]
    public async Task Delete_Doctor_Can_Delete_Own_And_Global_But_Not_Other_Doctor()
    {
        await using var db = CreateDbContext();
        var data = await SeedAsync(db);
        var service = new ExerciseService(db, new TestMediaStorageService());

        await service.SoftDeleteExerciseAsync(data.ClinicAId, data.DoctorAId, data.OwnExerciseId, isAdmin: false);
        await service.SoftDeleteExerciseAsync(data.ClinicAId, data.DoctorAId, data.GlobalExerciseId, isAdmin: false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.SoftDeleteExerciseAsync(data.ClinicAId, data.DoctorAId, data.OtherDoctorExerciseId, isAdmin: false));

        var own = await db.Exercises.SingleAsync(x => x.Id == data.OwnExerciseId);
        var global = await db.Exercises.SingleAsync(x => x.Id == data.GlobalExerciseId);
        var other = await db.Exercises.SingleAsync(x => x.Id == data.OtherDoctorExerciseId);

        Assert.True(own.IsDeleted);
        Assert.True(global.IsDeleted);
        Assert.False(other.IsDeleted);
    }

    [Fact]
    public async Task Delete_Admin_Can_Delete_Any_In_Clinic_But_Not_Other_Clinic()
    {
        await using var db = CreateDbContext();
        var data = await SeedAsync(db);
        var service = new ExerciseService(db, new TestMediaStorageService());

        await service.SoftDeleteExerciseAsync(data.ClinicAId, data.AdminAId, data.OtherDoctorExerciseId, isAdmin: true);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.SoftDeleteExerciseAsync(data.ClinicAId, data.AdminAId, data.ClinicBExerciseId, isAdmin: true));

        var deleted = await db.Exercises.SingleAsync(x => x.Id == data.OtherDoctorExerciseId);
        var otherClinic = await db.Exercises.SingleAsync(x => x.Id == data.ClinicBExerciseId);

        Assert.True(deleted.IsDeleted);
        Assert.False(otherClinic.IsDeleted);
    }

    [Fact]
    public async Task Update_Doctor_Can_Update_Own_And_Global_But_Not_Other_Doctor()
    {
        await using var db = CreateDbContext();
        var data = await SeedAsync(db);
        var service = new ExerciseService(db, new TestMediaStorageService());

        var updatedOwn = await service.UpdateExerciseAsync(
            data.ClinicAId,
            data.DoctorAId,
            data.OwnExerciseId,
            isAdmin: false,
            new UpdateExerciseRequestDto
            {
                Name = "Own Updated",
                Description = "Own Updated Description",
                Steps = ["Step A"],
                Type = ExerciseType.Time
            });

        var updatedGlobal = await service.UpdateExerciseAsync(
            data.ClinicAId,
            data.DoctorAId,
            data.GlobalExerciseId,
            isAdmin: false,
            new UpdateExerciseRequestDto
            {
                Name = "Global Updated",
                Description = "Global Updated Description",
                Steps = ["Step B"],
                Type = ExerciseType.Repetition
            });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.UpdateExerciseAsync(
                data.ClinicAId,
                data.DoctorAId,
                data.OtherDoctorExerciseId,
                isAdmin: false,
                new UpdateExerciseRequestDto
                {
                    Name = "Forbidden",
                    Description = "Forbidden",
                    Steps = ["Step C"],
                    Type = ExerciseType.Repetition
                }));

        Assert.Equal("Own Updated", updatedOwn.Name);
        Assert.Equal("Global Updated", updatedGlobal.Name);
    }

    [Fact]
    public async Task Update_Admin_Can_Update_Any_In_Clinic()
    {
        await using var db = CreateDbContext();
        var data = await SeedAsync(db);
        var service = new ExerciseService(db, new TestMediaStorageService());

        var updated = await service.UpdateExerciseAsync(
            data.ClinicAId,
            data.AdminAId,
            data.OtherDoctorExerciseId,
            isAdmin: true,
            new UpdateExerciseRequestDto
            {
                Name = "Admin Updated",
                Description = "Admin Updated Description",
                Steps = ["Step Z"],
                Type = ExerciseType.Time
            });

        Assert.Equal("Admin Updated", updated.Name);
        Assert.Equal(ExerciseType.Time, updated.Type);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"exercise-permissions-{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }

    private static async Task<SeedData> SeedAsync(AppDbContext db)
    {
        var clinicA = new Clinic { Id = Guid.NewGuid(), Name = "Clinic A" };
        var clinicB = new Clinic { Id = Guid.NewGuid(), Name = "Clinic B" };

        var adminA = CreateUser(clinicA.Id, UserRole.Admin, "admin.a@clinic.local");
        var doctorA = CreateUser(clinicA.Id, UserRole.Doctor, "doctor.a@clinic.local");
        var doctorA2 = CreateUser(clinicA.Id, UserRole.Doctor, "doctor.a2@clinic.local");
        var doctorB = CreateUser(clinicB.Id, UserRole.Doctor, "doctor.b@clinic.local");

        var ownExercise = CreateExercise(clinicA.Id, doctorA.Id, "Own Exercise");
        var globalExercise = CreateExercise(clinicA.Id, null, "Global Exercise");
        var otherDoctorExercise = CreateExercise(clinicA.Id, doctorA2.Id, "Other Doctor Exercise");
        var clinicBExercise = CreateExercise(clinicB.Id, doctorB.Id, "Clinic B Exercise");

        db.Clinics.AddRange(clinicA, clinicB);
        db.Users.AddRange(adminA, doctorA, doctorA2, doctorB);
        db.Exercises.AddRange(ownExercise, globalExercise, otherDoctorExercise, clinicBExercise);

        await db.SaveChangesAsync();

        return new SeedData(
            clinicA.Id,
            adminA.Id,
            doctorA.Id,
            ownExercise.Id,
            globalExercise.Id,
            otherDoctorExercise.Id,
            clinicBExercise.Id);
    }

    private static User CreateUser(Guid clinicId, UserRole role, string email)
    {
        return new User
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

    private static Exercise CreateExercise(Guid clinicId, Guid? userId, string name)
    {
        return new Exercise
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            UserId = userId,
            Name = name,
            Description = "Description",
            MediaUrls = [],
            IsDeleted = false,
            Type = ExerciseType.Repetition,
            Steps = ["Step 1"]
        };
    }

    private sealed record SeedData(
        Guid ClinicAId,
        Guid AdminAId,
        Guid DoctorAId,
        Guid OwnExerciseId,
        Guid GlobalExerciseId,
        Guid OtherDoctorExerciseId,
        Guid ClinicBExerciseId);

    private sealed class TestMediaStorageService : IMediaStorageService
    {
        public Task<string?> UploadAsync(string prefix, IFormFile? file, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<string?>(null);
        }

        public Task DeleteFileByUrlAsync(string? fileUrl, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}

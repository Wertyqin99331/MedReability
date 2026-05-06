using MedReability.Application.Interfaces.Storage;
using MedReability.Domain.Entities;
using MedReability.Domain.Enums;
using MedReability.Infrastructure.Persistence;
using MedReability.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace MedReability.Api.Tests.Users;

public class UserServiceTests
{
    [Fact]
    public async Task ListUsers_ReturnsHasActivePlanOnlyForPatientsWithUnfinishedPlans()
    {
        await using var db = CreateDbContext();
        var data = await SeedAsync(db, PatientTrainingPlanStatus.InProgress);
        var service = CreateService(db);

        var result = await service.ListUsersAsync(data.ClinicId, new());

        var patient = result.Items.Single(x => x.Id == data.PatientId);
        var doctor = result.Items.Single(x => x.Id == data.DoctorId);
        Assert.True(patient.HasActivePlan);
        Assert.False(doctor.HasActivePlan);
    }

    [Fact]
    public async Task DeactivateUser_PatientWithActivePlan_ThrowsInvalidOperationAndKeepsUserActive()
    {
        await using var db = CreateDbContext();
        var data = await SeedAsync(db, PatientTrainingPlanStatus.Assigned);
        var service = CreateService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.DeactivateUserAsync(data.ClinicId, data.PatientId));

        var patient = await db.Users.SingleAsync(x => x.Id == data.PatientId);
        Assert.True(patient.IsActive);
    }

    [Fact]
    public async Task DeactivateUser_PatientWithCompletedPlan_DeactivatesUser()
    {
        await using var db = CreateDbContext();
        var data = await SeedAsync(db, PatientTrainingPlanStatus.Completed);
        var service = CreateService(db);

        var result = await service.DeactivateUserAsync(data.ClinicId, data.PatientId);

        var patient = await db.Users.SingleAsync(x => x.Id == data.PatientId);
        Assert.True(result);
        Assert.False(patient.IsActive);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"user-service-tests-{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }

    private static UserService CreateService(AppDbContext dbContext)
    {
        return new UserService(dbContext, new TestMediaStorageService());
    }

    private static async Task<SeedData> SeedAsync(AppDbContext db, PatientTrainingPlanStatus planStatus)
    {
        var clinic = new ClinicEntity { Id = Guid.NewGuid(), Name = "Clinic A" };
        var doctor = CreateUser(clinic.Id, UserRole.Doctor, "doctor@clinic.local");
        var patient = CreateUser(clinic.Id, UserRole.Patient, "patient@clinic.local");

        db.Clinics.Add(clinic);
        db.Users.AddRange(doctor, patient);
        db.PatientTrainingPlans.Add(new PatientTrainingPlanEntity
        {
            Id = Guid.NewGuid(),
            ClinicId = clinic.Id,
            PatientId = patient.Id,
            CreatedByUserId = doctor.Id,
            Name = "Recovery Plan",
            StartDate = new DateOnly(2026, 5, 6),
            Status = planStatus,
            IsDeleted = false
        });
        await db.SaveChangesAsync();

        return new SeedData(clinic.Id, doctor.Id, patient.Id);
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

    private sealed record SeedData(Guid ClinicId, Guid DoctorId, Guid PatientId);

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

using MedReability.Application.Interfaces.Security;
using MedReability.Domain.Entities;
using MedReability.Domain.Enums;
using MedReability.Infrastructure.Persistence;
using MedReability.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace MedReability.Api.Tests.Assignments;

public class DoctorPatientAssignmentServiceTests
{
    [Fact]
    public async Task GetDoctorPatients_ReturnsPatientImageUrl()
    {
        await using var db = CreateDbContext();
        var clinic = new ClinicEntity { Id = Guid.NewGuid(), Name = "Clinic A" };
        var doctor = CreateUser(clinic.Id, UserRole.Doctor, "doctor@clinic.local", imageUrl: null);
        var patient = CreateUser(clinic.Id, UserRole.Patient, "patient@clinic.local", "https://cdn.local/patient.png");

        db.Clinics.Add(clinic);
        db.Users.AddRange(doctor, patient);
        db.DoctorPatientAssignments.Add(new DoctorPatientAssignmentEntity
        {
            Id = Guid.NewGuid(),
            ClinicId = clinic.Id,
            DoctorId = doctor.Id,
            PatientId = patient.Id
        });
        await db.SaveChangesAsync();

        var service = CreateService(db);

        var result = await service.GetDoctorPatientsAsync(clinic.Id, doctor.Id);

        var item = Assert.Single(result);
        Assert.Equal(patient.Id, item.PatientId);
        Assert.Equal(patient.ImageUrl, item.ImageUrl);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"doctor-patient-assignment-service-tests-{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }

    private static DoctorPatientAssignmentService CreateService(AppDbContext dbContext)
    {
        IAccessPolicyService accessPolicyService = new AccessPolicyService();
        return new DoctorPatientAssignmentService(dbContext, accessPolicyService);
    }

    private static UserEntity CreateUser(Guid clinicId, UserRole role, string email, string? imageUrl)
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
            ImageUrl = imageUrl,
            Role = role,
            IsActive = true
        };
    }
}

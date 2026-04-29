using MedReability.Domain.Entities;
using MedReability.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace MedReability.Api.Tests.Infrastructure;

public static class TestData
{
    public static readonly Guid Clinic1Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid Clinic2Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    public static readonly Guid Clinic1AdminId = Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111");
    public static readonly Guid Clinic2AdminId = Guid.Parse("bbbbbbbb-1111-1111-1111-111111111111");
    public static readonly Guid Clinic1PatientId = Guid.Parse("aaaaaaaa-2222-2222-2222-222222222222");
    public static readonly Guid Clinic2PatientId = Guid.Parse("bbbbbbbb-2222-2222-2222-222222222222");

    public const string SharedAdminEmail = "admin@tenant.local";
    public const string Clinic1AdminPassword = "AdminClinic1!";
    public const string Clinic2AdminPassword = "AdminClinic2!";
    public const string Clinic1PatientEmail = "patient1@tenant.local";
    public const string Clinic1PatientPassword = "PatientClinic1!";

    public static List<ClinicEntity> BuildClinics()
    {
        return
        [
            new ClinicEntity { Id = Clinic1Id, Name = "ClinicEntity One" },
            new ClinicEntity { Id = Clinic2Id, Name = "ClinicEntity Two" }
        ];
    }

    public static List<UserEntity> BuildUsers()
    {
        var hasher = new PasswordHasher<UserEntity>();

        var clinic1Admin = CreateUser(
            Clinic1AdminId,
            Clinic1Id,
            SharedAdminEmail,
            Clinic1AdminPassword,
            "Alice",
            "Ivanovna",
            "Admin",
            "+79000001001",
            UserRole.Admin);

        var clinic2Admin = CreateUser(
            Clinic2AdminId,
            Clinic2Id,
            SharedAdminEmail,
            Clinic2AdminPassword,
            "Bob",
            "Petrovich",
            "Admin",
            "+79000001002",
            UserRole.Admin);

        var clinic1Patient = CreateUser(
            Clinic1PatientId,
            Clinic1Id,
            Clinic1PatientEmail,
            Clinic1PatientPassword,
            "Pat",
            "Sergeevna",
            "One",
            "+79000001003",
            UserRole.Patient);

        var clinic2Patient = CreateUser(
            Clinic2PatientId,
            Clinic2Id,
            "patient2@tenant.local",
            "PatientClinic2!",
            "Pat",
            "Alekseevna",
            "Two",
            "+79000001004",
            UserRole.Patient);

        foreach (var user in new[] { clinic1Admin, clinic2Admin, clinic1Patient, clinic2Patient })
        {
            user.PasswordHash = hasher.HashPassword(user, user.PasswordHash);
        }

        return [clinic1Admin, clinic2Admin, clinic1Patient, clinic2Patient];
    }

    private static UserEntity CreateUser(
        Guid id,
        Guid clinicId,
        string email,
        string rawPassword,
        string firstName,
        string patronymic,
        string lastName,
        string phoneNumber,
        UserRole role)
    {
        return new UserEntity
        {
            Id = id,
            ClinicId = clinicId,
            Email = email.ToLowerInvariant(),
            PasswordHash = rawPassword,
            FirstName = firstName,
            Patronymic = patronymic,
            LastName = lastName,
            PhoneNumber = phoneNumber,
            Role = role,
            IsActive = true
        };
    }
}

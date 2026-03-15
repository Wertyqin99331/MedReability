using MedReability.Domain.Entities;
using MedReability.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MedReability.Infrastructure.Persistence;

public class DataSeeder(AppDbContext dbContext)
{
    private static readonly Guid SeedClinicId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid SeedClinic2Id = Guid.Parse("55555555-5555-5555-5555-555555555555");
    private static readonly Guid SeedAdminId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid SeedDoctorId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid SeedPatientId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid SeedClinic2AdminId = Guid.Parse("66666666-6666-6666-6666-666666666666");

    private readonly PasswordHasher<User> _passwordHasher = new();

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!await dbContext.Clinics.AnyAsync(x => x.Id == SeedClinicId, cancellationToken))
        {
            dbContext.Clinics.Add(new Clinic
            {
                Id = SeedClinicId,
                Name = "Default Clinic"
            });
        }

        if (!await dbContext.Clinics.AnyAsync(x => x.Id == SeedClinic2Id, cancellationToken))
        {
            dbContext.Clinics.Add(new Clinic
            {
                Id = SeedClinic2Id,
                Name = "Second Clinic"
            });
        }

        if (!await dbContext.Users.AnyAsync(x => x.Id == SeedAdminId, cancellationToken))
        {
            dbContext.Users.Add(CreateUser(
                SeedAdminId,
                SeedClinicId,
                "admin@clinic.local",
                "Admin123!",
                "Clinic",
                "Admin",
                UserRole.Admin));
        }

        if (!await dbContext.Users.AnyAsync(x => x.Id == SeedDoctorId, cancellationToken))
        {
            dbContext.Users.Add(CreateUser(
                SeedDoctorId,
                SeedClinicId,
                "doctor@clinic.local",
                "Doctor123!",
                "Default",
                "Doctor",
                UserRole.Doctor));
        }

        if (!await dbContext.Users.AnyAsync(x => x.Id == SeedPatientId, cancellationToken))
        {
            dbContext.Users.Add(CreateUser(
                SeedPatientId,
                SeedClinicId,
                "patient@clinic.local",
                "Patient123!",
                "Default",
                "Patient",
                UserRole.Patient));
        }

        if (!await dbContext.Users.AnyAsync(x => x.Id == SeedClinic2AdminId, cancellationToken))
        {
            dbContext.Users.Add(CreateUser(
                SeedClinic2AdminId,
                SeedClinic2Id,
                "admin@second-clinic.local",
                "Admin123!",
                "Second",
                "Admin",
                UserRole.Admin));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private User CreateUser(
        Guid id,
        Guid clinicId,
        string email,
        string password,
        string firstName,
        string lastName,
        UserRole role)
    {
        var user = new User
        {
            Id = id,
            ClinicId = clinicId,
            Email = email.ToLowerInvariant(),
            FirstName = firstName,
            LastName = lastName,
            Role = role,
            IsActive = true
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, password);
        return user;
    }
}

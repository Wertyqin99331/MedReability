using MedReability.Application.DTOs.Assignments;
using MedReability.Application.DTOs.Common;
using MedReability.Application.Interfaces.Services;
using MedReability.Domain.Entities;
using MedReability.Domain.Enums;
using MedReability.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MedReability.Infrastructure.Services;

public class DoctorPatientAssignmentService(AppDbContext dbContext) : IDoctorPatientAssignmentService
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
                x => x.Id == doctorId && x.ClinicId == clinicId && x.IsActive,
                cancellationToken);

        if (doctor is null)
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
                    IsActive = patient.IsActive
                })
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ToListAsync(cancellationToken);
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
            .FirstOrDefaultAsync(x => x.Id == assignmentId && x.ClinicId == clinicId, cancellationToken);

        if (assignment is null)
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
                x => x.Id == doctorId && x.ClinicId == clinicId && x.IsActive,
                cancellationToken);

        if (doctor is null)
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
                x => x.Id == patientId && x.ClinicId == clinicId && x.IsActive,
                cancellationToken);

        if (patient is null)
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
}

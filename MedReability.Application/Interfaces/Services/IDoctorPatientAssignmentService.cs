using MedReability.Application.DTOs.Assignments;
using MedReability.Application.DTOs.Common;

namespace MedReability.Application.Interfaces.Services;

public interface IDoctorPatientAssignmentService
{
    Task<DoctorPatientAssignmentResponseDto> AssignAsync(
        Guid clinicId,
        Guid doctorId,
        Guid patientId,
        CancellationToken cancellationToken = default);

    Task<List<DoctorPatientListItemDto>> GetDoctorPatientsAsync(
        Guid clinicId,
        Guid doctorId,
        CancellationToken cancellationToken = default);

    Task<PagedResultDto<DoctorPatientAssignmentListItemDto>> GetAssignmentsAsync(
        Guid clinicId,
        DoctorPatientAssignmentsQueryDto query,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid clinicId,
        Guid assignmentId,
        CancellationToken cancellationToken = default);
}

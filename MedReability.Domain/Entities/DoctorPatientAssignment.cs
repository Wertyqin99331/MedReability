namespace MedReability.Domain.Entities;

public class DoctorPatientAssignment
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Clinic Clinic { get; set; } = null!;

    public Guid DoctorId { get; set; }
    public User Doctor { get; set; } = null!;

    public Guid PatientId { get; set; }
    public User Patient { get; set; } = null!;
}

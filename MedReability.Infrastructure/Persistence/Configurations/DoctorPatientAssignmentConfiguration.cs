using MedReability.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedReability.Infrastructure.Persistence.Configurations;

public class DoctorPatientAssignmentConfiguration : IEntityTypeConfiguration<DoctorPatientAssignmentEntity>
{
    public void Configure(EntityTypeBuilder<DoctorPatientAssignmentEntity> builder)
    {
        builder.ToTable("doctor_patient_assignments");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.ClinicId)
            .HasColumnName("clinic_id")
            .IsRequired();

        builder.Property(x => x.DoctorId)
            .HasColumnName("doctor_id")
            .IsRequired();

        builder.Property(x => x.PatientId)
            .HasColumnName("patient_id")
            .IsRequired();

        builder.HasIndex(x => new { x.ClinicId, x.DoctorId, x.PatientId })
            .IsUnique();

        builder.HasOne(x => x.ClinicEntity)
            .WithMany()
            .HasForeignKey(x => x.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Doctor)
            .WithMany()
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Patient)
            .WithMany()
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

using MedReability.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedReability.Infrastructure.Persistence.Configurations;

public class PatientTrainingPlanDayExerciseProgressConfiguration
    : IEntityTypeConfiguration<PatientTrainingPlanDayExerciseProgressEntity>
{
    public void Configure(EntityTypeBuilder<PatientTrainingPlanDayExerciseProgressEntity> builder)
    {
        builder.ToTable("patient_training_plan_day_exercise_progresses");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.PatientId)
            .HasColumnName("patient_id")
            .IsRequired();

        builder.Property(x => x.PatientTrainingPlanId)
            .HasColumnName("patient_training_plan_id")
            .IsRequired();

        builder.Property(x => x.DayNumber)
            .HasColumnName("day_number")
            .IsRequired();

        builder.Property(x => x.PatientTrainingPlanDayExerciseId)
            .HasColumnName("patient_training_plan_day_exercise_id")
            .IsRequired();

        builder.Property(x => x.CompletedAtUtc)
            .HasColumnName("completed_at_utc")
            .IsRequired();

        builder.HasIndex(x => new
            {
                x.PatientId,
                x.PatientTrainingPlanId,
                x.DayNumber,
                x.PatientTrainingPlanDayExerciseId
            })
            .IsUnique()
            .HasDatabaseName("UX_ptp_day_ex_progress_patient_plan_day_exercise");

        builder.HasIndex(x => x.PatientTrainingPlanId)
            .HasDatabaseName("IX_ptp_day_ex_progress_plan");
        builder.HasIndex(x => x.PatientTrainingPlanDayExerciseId)
            .HasDatabaseName("IX_ptp_day_ex_progress_day_exercise");

        builder.HasOne(x => x.Patient)
            .WithMany()
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_ptp_day_ex_progress_patient");

        builder.HasOne(x => x.PatientTrainingPlanEntity)
            .WithMany()
            .HasForeignKey(x => x.PatientTrainingPlanId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_ptp_day_ex_progress_plan");

        builder.HasOne(x => x.PatientTrainingPlanDayExerciseEntity)
            .WithMany()
            .HasForeignKey(x => x.PatientTrainingPlanDayExerciseId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_ptp_day_ex_progress_day_exercise");
    }
}

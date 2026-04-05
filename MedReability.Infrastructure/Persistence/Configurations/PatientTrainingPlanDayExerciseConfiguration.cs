using MedReability.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedReability.Infrastructure.Persistence.Configurations;

public class PatientTrainingPlanDayExerciseConfiguration : IEntityTypeConfiguration<PatientTrainingPlanDayExercise>
{
    public void Configure(EntityTypeBuilder<PatientTrainingPlanDayExercise> builder)
    {
        builder.ToTable("patient_training_plan_day_exercises");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.PatientTrainingPlanDayId)
            .HasColumnName("patient_training_plan_day_id")
            .IsRequired();

        builder.Property(x => x.Order)
            .HasColumnName("order")
            .IsRequired();

        builder.Property(x => x.ExerciseId)
            .HasColumnName("exercise_id")
            .IsRequired();

        builder.Property(x => x.Repetitions)
            .HasColumnName("repetitions")
            .IsRequired(false);

        builder.Property(x => x.DurationSeconds)
            .HasColumnName("duration_seconds")
            .IsRequired(false);

        builder.Property(x => x.Comment)
            .HasColumnName("comment")
            .HasMaxLength(2000)
            .IsRequired(false);

        builder.HasIndex(x => new { x.PatientTrainingPlanDayId, x.Order })
            .IsUnique();

        builder.HasIndex(x => x.ExerciseId);

        builder.ToTable(x => x.HasCheckConstraint(
            "CK_patient_training_plan_day_exercises_prescription",
            "(repetitions IS NOT NULL AND duration_seconds IS NULL) OR (repetitions IS NULL AND duration_seconds IS NOT NULL)"));

        builder.HasOne(x => x.PatientTrainingPlanDay)
            .WithMany(x => x.Exercises)
            .HasForeignKey(x => x.PatientTrainingPlanDayId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Exercise)
            .WithMany()
            .HasForeignKey(x => x.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

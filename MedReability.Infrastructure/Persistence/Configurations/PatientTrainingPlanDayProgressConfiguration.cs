using MedReability.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedReability.Infrastructure.Persistence.Configurations;

public class PatientTrainingPlanDayProgressConfiguration : IEntityTypeConfiguration<PatientTrainingPlanDayProgressEntity>
{
    public void Configure(EntityTypeBuilder<PatientTrainingPlanDayProgressEntity> builder)
    {
        builder.ToTable("patient_training_plan_day_progresses");
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

        builder.Property(x => x.WellBeingRating)
            .HasColumnName("well_being_rating")
            .IsRequired(false);

        builder.Property(x => x.WorkoutDifficultyRating)
            .HasColumnName("workout_difficulty_rating")
            .IsRequired(false);

        builder.Property(x => x.HadPain)
            .HasColumnName("had_pain")
            .IsRequired(false);

        builder.Property(x => x.PainIntensityRating)
            .HasColumnName("pain_intensity_rating")
            .IsRequired(false);

        builder.Property(x => x.CompletedAtUtc)
            .HasColumnName("completed_at_utc")
            .IsRequired();

        builder.HasIndex(x => new { x.PatientId, x.PatientTrainingPlanId, x.DayNumber })
            .IsUnique();

        builder.HasIndex(x => x.PatientTrainingPlanId);

        builder.ToTable(x => x.HasCheckConstraint(
            "CK_ptp_day_progress_well_being_rating",
            "well_being_rating IS NULL OR (well_being_rating >= 1 AND well_being_rating <= 10)"));

        builder.ToTable(x => x.HasCheckConstraint(
            "CK_ptp_day_progress_workout_difficulty_rating",
            "workout_difficulty_rating IS NULL OR (workout_difficulty_rating >= 1 AND workout_difficulty_rating <= 10)"));

        builder.ToTable(x => x.HasCheckConstraint(
            "CK_ptp_day_progress_pain_intensity_rating",
            "pain_intensity_rating IS NULL OR (pain_intensity_rating >= 1 AND pain_intensity_rating <= 10)"));

        builder.ToTable(x => x.HasCheckConstraint(
            "CK_ptp_day_progress_pain_consistency",
            "(had_pain IS TRUE AND pain_intensity_rating IS NOT NULL) OR (had_pain IS NOT TRUE AND pain_intensity_rating IS NULL)"));

        builder.HasOne(x => x.Patient)
            .WithMany()
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PatientTrainingPlanEntity)
            .WithMany()
            .HasForeignKey(x => x.PatientTrainingPlanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

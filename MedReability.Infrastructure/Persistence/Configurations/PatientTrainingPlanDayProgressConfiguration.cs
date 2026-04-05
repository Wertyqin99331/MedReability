using MedReability.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedReability.Infrastructure.Persistence.Configurations;

public class PatientTrainingPlanDayProgressConfiguration : IEntityTypeConfiguration<PatientTrainingPlanDayProgress>
{
    public void Configure(EntityTypeBuilder<PatientTrainingPlanDayProgress> builder)
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

        builder.Property(x => x.StateRating)
            .HasColumnName("state_rating")
            .IsRequired(false);

        builder.Property(x => x.Notes)
            .HasColumnName("notes")
            .HasMaxLength(2000)
            .IsRequired(false);

        builder.Property(x => x.CompletedAtUtc)
            .HasColumnName("completed_at_utc")
            .IsRequired();

        builder.HasIndex(x => new { x.PatientId, x.PatientTrainingPlanId, x.DayNumber })
            .IsUnique();

        builder.HasIndex(x => x.PatientTrainingPlanId);

        builder.ToTable(x => x.HasCheckConstraint(
            "CK_patient_training_plan_day_progresses_state_rating",
            "state_rating IS NULL OR (state_rating >= 1 AND state_rating <= 5)"));

        builder.HasOne(x => x.Patient)
            .WithMany()
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PatientTrainingPlan)
            .WithMany()
            .HasForeignKey(x => x.PatientTrainingPlanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

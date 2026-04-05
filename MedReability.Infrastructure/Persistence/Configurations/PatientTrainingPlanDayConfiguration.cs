using MedReability.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedReability.Infrastructure.Persistence.Configurations;

public class PatientTrainingPlanDayConfiguration : IEntityTypeConfiguration<PatientTrainingPlanDay>
{
    public void Configure(EntityTypeBuilder<PatientTrainingPlanDay> builder)
    {
        builder.ToTable("patient_training_plan_days");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.PatientTrainingPlanId)
            .HasColumnName("patient_training_plan_id")
            .IsRequired();

        builder.Property(x => x.DayNumber)
            .HasColumnName("day_number")
            .IsRequired();

        builder.Property(x => x.IsRestDay)
            .HasColumnName("is_rest_day")
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasColumnName("notes")
            .HasMaxLength(2000)
            .IsRequired(false);

        builder.HasIndex(x => new { x.PatientTrainingPlanId, x.DayNumber })
            .IsUnique();

        builder.HasOne(x => x.PatientTrainingPlan)
            .WithMany(x => x.Days)
            .HasForeignKey(x => x.PatientTrainingPlanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

using MedReability.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedReability.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ClinicEntity> Clinics => Set<ClinicEntity>();
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<DoctorPatientAssignmentEntity> DoctorPatientAssignments => Set<DoctorPatientAssignmentEntity>();
    public DbSet<ExerciseEntity> Exercises => Set<ExerciseEntity>();
    public DbSet<PatientTrainingPlanEntity> PatientTrainingPlans => Set<PatientTrainingPlanEntity>();
    public DbSet<PatientTrainingPlanDayEntity> PatientTrainingPlanDays => Set<PatientTrainingPlanDayEntity>();
    public DbSet<PatientTrainingPlanDayExerciseEntity> PatientTrainingPlanDayExercises => Set<PatientTrainingPlanDayExerciseEntity>();
    public DbSet<PatientTrainingPlanDayProgressEntity> PatientTrainingPlanDayProgresses => Set<PatientTrainingPlanDayProgressEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

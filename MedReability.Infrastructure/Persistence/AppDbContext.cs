using MedReability.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedReability.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<User> Users => Set<User>();
    public DbSet<DoctorPatientAssignment> DoctorPatientAssignments => Set<DoctorPatientAssignment>();
    public DbSet<Exercise> Exercises => Set<Exercise>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

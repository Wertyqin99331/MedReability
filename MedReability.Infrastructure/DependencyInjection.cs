using MedReability.Application.Interfaces.Security;
using MedReability.Application.Interfaces.Services;
using MedReability.Infrastructure.Persistence;
using MedReability.Infrastructure.Security;
using MedReability.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MedReability.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var useInMemory = bool.TryParse(configuration["Testing:UseInMemoryDatabase"], out var parsed) && parsed;

        services.AddDbContext<AppDbContext>(options =>
        {
            if (useInMemory)
            {
                var dbName = configuration["Testing:DatabaseName"] ?? "medreability-tests";
                options.UseInMemoryDatabase(dbName);
                return;
            }

            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
        });

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IClinicService, ClinicService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<DataSeeder>();

        return services;
    }
}

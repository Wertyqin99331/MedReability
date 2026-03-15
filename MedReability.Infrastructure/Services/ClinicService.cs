using MedReability.Application.DTOs.Clinics;
using MedReability.Application.Interfaces.Services;
using MedReability.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MedReability.Infrastructure.Services;

public class ClinicService(AppDbContext dbContext) : IClinicService
{
    public async Task<List<ClinicResponseDto>> GetClinicsAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Clinics
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new ClinicResponseDto
            {
                Id = x.Id,
                Name = x.Name
            })
            .ToListAsync(cancellationToken);
    }
}

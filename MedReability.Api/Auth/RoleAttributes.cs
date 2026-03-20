using MedReability.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace MedReability.Api.Auth;

public sealed class AdminOnlyAttribute : AuthorizeAttribute
{
    public AdminOnlyAttribute()
    {
        Roles = nameof(UserRole.Admin);
    }
}

public sealed class DoctorOnlyAttribute : AuthorizeAttribute
{
    public DoctorOnlyAttribute()
    {
        Roles = nameof(UserRole.Doctor);
    }
}

public sealed class PatientOnlyAttribute : AuthorizeAttribute
{
    public PatientOnlyAttribute()
    {
        Roles = nameof(UserRole.Patient);
    }
}

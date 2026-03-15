using System.Security.Claims;
using MedReability.Application.Common;

namespace MedReability.Api.Common;

public static class HttpContextExtensions
{
    public static Guid? GetClinicId(this ClaimsPrincipal user)
    {
        var clinicIdValue = user.FindFirst(AuthClaimTypes.ClinicId)?.Value;
        return Guid.TryParse(clinicIdValue, out var clinicId) ? clinicId : null;
    }
}

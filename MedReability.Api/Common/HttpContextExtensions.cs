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

    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var userIdValue = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? user.FindFirst("sub")?.Value;
        return Guid.TryParse(userIdValue, out var userId) ? userId : null;
    }
}

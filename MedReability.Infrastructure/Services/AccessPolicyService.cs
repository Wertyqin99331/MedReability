using MedReability.Application.Interfaces.Security;

namespace MedReability.Infrastructure.Services;

public class AccessPolicyService : IAccessPolicyService
{
    public bool IsSameClinic(
        Guid currentClinicId,
        Guid? resourceClinicId)
    {
        return resourceClinicId == currentClinicId;
    }

    public bool IsSameClinicAndAdmin(
        Guid currentClinicId,
        Guid? resourceClinicId,
        bool isAdmin)
    {
        return IsSameClinic(currentClinicId, resourceClinicId) && isAdmin;
    }

    public bool IsSameClinicAndAdminOrDoctor(
        Guid currentClinicId,
        Guid? resourceClinicId,
        bool isAdmin,
        bool isDoctor)
    {
        return IsSameClinic(currentClinicId, resourceClinicId) && (isAdmin || isDoctor);
    }

    public bool IsAdminOrOwner(
        bool isAdmin,
        Guid currentUserId,
        Guid? ownerUserId)
    {
        if (isAdmin)
        {
            return true;
        }

        return ownerUserId == currentUserId;
    }

    public bool IsAdminOrOwnerOrGlobal(
        bool isAdmin,
        Guid currentUserId,
        Guid? ownerUserId)
    {
        return isAdmin || ownerUserId is null || ownerUserId == currentUserId;
    }
}

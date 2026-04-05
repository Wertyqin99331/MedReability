namespace MedReability.Application.Interfaces.Security;

public interface IAccessPolicyService
{
    bool IsSameClinic(
        Guid currentClinicId,
        Guid? resourceClinicId);

    bool IsSameClinicAndAdmin(
        Guid currentClinicId,
        Guid? resourceClinicId,
        bool isAdmin);

    bool IsSameClinicAndAdminOrDoctor(
        Guid currentClinicId,
        Guid? resourceClinicId,
        bool isAdmin,
        bool isDoctor);

    bool IsAdminOrOwner(
        bool isAdmin,
        Guid currentUserId,
        Guid? ownerUserId);

    bool IsAdminOrOwnerOrGlobal(
        bool isAdmin,
        Guid currentUserId,
        Guid? ownerUserId);
}

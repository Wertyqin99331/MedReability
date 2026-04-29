using MedReability.Domain.Entities;

namespace MedReability.Application.Interfaces.Security;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAtUtc) Generate(UserEntity user);
}

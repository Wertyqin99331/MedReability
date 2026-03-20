using MedReability.Domain.Enums;

namespace MedReability.Application.DTOs.Users;

public class ListUsersQueryDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
    public List<UserRole>? Roles { get; set; }
}

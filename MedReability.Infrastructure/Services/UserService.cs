using MedReability.Application.DTOs.Common;
using MedReability.Application.DTOs.Users;
using MedReability.Application.Interfaces.Services;
using MedReability.Application.Interfaces.Storage;
using MedReability.Domain.Entities;
using MedReability.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MedReability.Infrastructure.Services;

public class UserService(
    AppDbContext dbContext,
    IMediaStorageService mediaStorageService) : IUserService
{
    private readonly PasswordHasher<UserEntity> _passwordHasher = new();

    public async Task<UserResponseDto> CreateUserAsync(Guid clinicId, CreateUserRequestDto request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var emailExists = await dbContext.Users
            .WithClinicEntity(x => x.ClinicId, clinicId)
            .AnyAsync(x => x.Email == email, cancellationToken);
        if (emailExists)
        {
            throw new InvalidOperationException("UserEntity with this email already exists.");
        }

        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            Email = email,
            FirstName = request.FirstName.Trim(),
            Patronymic = request.Patronymic.Trim(),
            LastName = request.LastName.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            ImageUrl = await mediaStorageService.UploadAsync("users", request.Image, cancellationToken),
            Role = request.Role,
            IsActive = true
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(user);
    }

    public async Task<PagedResultDto<UserResponseDto>> ListUsersAsync(Guid clinicId, ListUsersQueryDto query, CancellationToken cancellationToken = default)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize switch
        {
            < 1 => 20,
            > 100 => 100,
            _ => query.PageSize
        };

        var usersQuery = dbContext.Users
            .WithClinicEntity(x => x.ClinicId, clinicId);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLowerInvariant();
            usersQuery = usersQuery.Where(x =>
                x.FirstName.ToLower().Contains(search) ||
                x.LastName.ToLower().Contains(search) ||
                x.Patronymic.ToLower().Contains(search));
        }

        if (query.Roles is { Count: > 0 })
        {
            usersQuery = usersQuery.Where(x => query.Roles.Contains(x.Role));
        }

        usersQuery = usersQuery
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ThenBy(x => x.Email);

        var totalCount = await usersQuery.CountAsync(cancellationToken);

        var users = await usersQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new UserResponseDto
            {
                Id = x.Id,
                ClinicId = x.ClinicId,
                Email = x.Email,
                FirstName = x.FirstName,
                Patronymic = x.Patronymic,
                LastName = x.LastName,
                PhoneNumber = x.PhoneNumber,
                ImageUrl = x.ImageUrl,
                Role = x.Role,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<UserResponseDto>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = users
        };
    }

    public async Task<UserResponseDto> UpdateUserProfileAsync(
        Guid clinicId,
        Guid userId,
        UpdateUserProfileRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .WithClinicEntity(x => x.ClinicId, clinicId)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null)
        {
            throw new KeyNotFoundException("User was not found in your clinic.");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var emailExists = await dbContext.Users
            .WithClinicEntity(x => x.ClinicId, clinicId)
            .AnyAsync(x => x.Email == normalizedEmail && x.Id != userId, cancellationToken);
        if (emailExists)
        {
            throw new InvalidOperationException("User with this email already exists.");
        }

        user.Email = normalizedEmail;
        user.FirstName = request.FirstName.Trim();
        user.Patronymic = request.Patronymic.Trim();
        user.LastName = request.LastName.Trim();
        user.PhoneNumber = request.PhoneNumber.Trim();

        if (request.Image is not null && request.Image.Length > 0)
        {
            var previousImageUrl = user.ImageUrl;
            user.ImageUrl = await mediaStorageService.UploadAsync("users", request.Image, cancellationToken);

            if (!string.IsNullOrWhiteSpace(previousImageUrl))
            {
                await mediaStorageService.DeleteFileByUrlAsync(previousImageUrl, cancellationToken);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(user);
    }

    public async Task SetUserPasswordAsync(
        Guid clinicId,
        Guid userId,
        SetUserPasswordRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .WithClinicEntity(x => x.ClinicId, clinicId)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null)
        {
            throw new KeyNotFoundException("User was not found in your clinic.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeactivateUserAsync(Guid clinicId, Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .WithClinicEntity(x => x.ClinicId, clinicId)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null)
        {
            return false;
        }

        if (!user.IsActive)
        {
            return true;
        }

        user.IsActive = false;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ActivateUserAsync(Guid clinicId, Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .WithClinicEntity(x => x.ClinicId, clinicId)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null)
        {
            return false;
        }

        if (user.IsActive)
        {
            return true;
        }

        user.IsActive = true;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static UserResponseDto Map(UserEntity user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            ClinicId = user.ClinicId,
            Email = user.Email,
            FirstName = user.FirstName,
            Patronymic = user.Patronymic,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            ImageUrl = user.ImageUrl,
            Role = user.Role,
            IsActive = user.IsActive
        };
    }
}

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using MedReability.Application.DTOs.Auth;
using MedReability.Application.DTOs.Common;
using MedReability.Application.DTOs.Users;
using MedReability.Api.Tests.Infrastructure;

namespace MedReability.Api.Tests.Tenancy;

public class TenancyIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _client;

    public TenancyIntegrationTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_Uses_ClinicId_And_Email()
    {
        var success = await _client.PostAsJsonAsync("api/auth/login", new LoginRequestDto
        {
            ClinicId = TestData.Clinic1Id,
            Email = TestData.SharedAdminEmail,
            Password = TestData.Clinic1AdminPassword
        });

        Assert.Equal(HttpStatusCode.OK, success.StatusCode);

        var failure = await _client.PostAsJsonAsync("api/auth/login", new LoginRequestDto
        {
            ClinicId = TestData.Clinic2Id,
            Email = TestData.SharedAdminEmail,
            Password = TestData.Clinic1AdminPassword
        });

        Assert.Equal(HttpStatusCode.Unauthorized, failure.StatusCode);
    }

    [Fact]
    public async Task Users_List_Is_Isolated_By_Clinic()
    {
        await AuthenticateAsync(TestData.Clinic1Id, TestData.SharedAdminEmail, TestData.Clinic1AdminPassword);

        var response = await _client.GetAsync("api/users?pageNumber=1&pageSize=50");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<PagedResultDto<UserResponseDto>>(JsonOptions);
        Assert.NotNull(payload);
        Assert.All(payload.Items, item => Assert.Equal(TestData.Clinic1Id, item.ClinicId));
        Assert.DoesNotContain(payload.Items, x => x.Id == TestData.Clinic2AdminId || x.Id == TestData.Clinic2PatientId);
    }

    [Fact]
    public async Task CrossClinic_Deactivate_Returns_NotFound()
    {
        await AuthenticateAsync(TestData.Clinic1Id, TestData.SharedAdminEmail, TestData.Clinic1AdminPassword);

        var response = await _client.PatchAsync($"api/users/{TestData.Clinic2PatientId}/deactivate", content: null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Duplicate_Email_Is_Allowed_Across_Clinics_But_Not_Inside_One_Clinic()
    {
        const string duplicateEmail = "dup@tenant.local";

        await AuthenticateAsync(TestData.Clinic1Id, TestData.SharedAdminEmail, TestData.Clinic1AdminPassword);

        var createInClinic1 = await _client.PostAsJsonAsync("api/users", new CreateUserRequestDto
        {
            Email = duplicateEmail,
            Password = "Password123!",
            FirstName = "Dupe",
            Patronymic = "Oneovich",
            LastName = "One",
            PhoneNumber = "+79000002001",
            Role = MedReability.Domain.Enums.UserRole.Patient
        });

        Assert.Equal(HttpStatusCode.Created, createInClinic1.StatusCode);

        var duplicateInClinic1 = await _client.PostAsJsonAsync("api/users", new CreateUserRequestDto
        {
            Email = duplicateEmail,
            Password = "Password123!",
            FirstName = "Dupe",
            Patronymic = "Againovich",
            LastName = "Again",
            PhoneNumber = "+79000002002",
            Role = MedReability.Domain.Enums.UserRole.Patient
        });

        Assert.Equal(HttpStatusCode.BadRequest, duplicateInClinic1.StatusCode);

        await AuthenticateAsync(TestData.Clinic2Id, TestData.SharedAdminEmail, TestData.Clinic2AdminPassword);

        var createInClinic2 = await _client.PostAsJsonAsync("api/users", new CreateUserRequestDto
        {
            Email = duplicateEmail,
            Password = "Password123!",
            FirstName = "Dupe",
            Patronymic = "Twovich",
            LastName = "Two",
            PhoneNumber = "+79000002003",
            Role = MedReability.Domain.Enums.UserRole.Patient
        });

        Assert.Equal(HttpStatusCode.Created, createInClinic2.StatusCode);
    }

    [Fact]
    public async Task Inactive_User_Cannot_Login()
    {
        await AuthenticateAsync(TestData.Clinic1Id, TestData.SharedAdminEmail, TestData.Clinic1AdminPassword);

        var deactivate = await _client.PatchAsync($"api/users/{TestData.Clinic1PatientId}/deactivate", content: null);
        Assert.Equal(HttpStatusCode.NoContent, deactivate.StatusCode);

        var login = await _client.PostAsJsonAsync("api/auth/login", new LoginRequestDto
        {
            ClinicId = TestData.Clinic1Id,
            Email = TestData.Clinic1PatientEmail,
            Password = TestData.Clinic1PatientPassword
        });

        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }

    private async Task AuthenticateAsync(Guid clinicId, string email, string password)
    {
        var response = await _client.PostAsJsonAsync("api/auth/login", new LoginRequestDto
        {
            ClinicId = clinicId,
            Email = email,
            Password = password
        });

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<LoginResponseDto>(JsonOptions);
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.AccessToken));

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body.AccessToken);
    }
}

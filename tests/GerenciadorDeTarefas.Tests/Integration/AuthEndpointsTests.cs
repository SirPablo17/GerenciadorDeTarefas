using System.Net;
using System.Net.Http.Json;
using GerenciadorDeTarefas.Application.DTOs;

namespace GerenciadorDeTarefas.Tests.Integration;

public class AuthEndpointsTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_WithNewEmail_ReturnsCreated()
    {
        var request = new RegisterRequest { Email = $"{Guid.NewGuid()}@test.com", Password = "Password123" };

        var response = await _client.PostAsJsonAsync("/auth/register", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        var email = $"{Guid.NewGuid()}@test.com";
        var request = new RegisterRequest { Email = email, Password = "Password123" };
        await _client.PostAsJsonAsync("/auth/register", request);

        var response = await _client.PostAsJsonAsync("/auth/register", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var email = $"{Guid.NewGuid()}@test.com";
        await _client.PostAsJsonAsync("/auth/register", new RegisterRequest { Email = email, Password = "Password123" });

        var response = await _client.PostAsJsonAsync("/auth/login", new LoginRequest { Email = email, Password = "Password123" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.False(string.IsNullOrWhiteSpace(body?.Token));
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/auth/login", new LoginRequest { Email = $"{Guid.NewGuid()}@test.com", Password = "WrongPassword1" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TasksEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/tasks");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

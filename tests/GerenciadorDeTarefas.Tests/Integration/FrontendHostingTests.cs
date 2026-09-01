using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GerenciadorDeTarefas.Application.DTOs;

namespace GerenciadorDeTarefas.Tests.Integration;

public class FrontendHostingTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = factory.CreateClient();
        var email = $"{Guid.NewGuid()}@test.com";
        const string password = "Password123";

        await client.PostAsJsonAsync("/auth/register", new RegisterRequest { Email = email, Password = password });
        var loginResponse = await client.PostAsJsonAsync("/auth/login", new LoginRequest { Email = email, Password = password });
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);
        return client;
    }

    [Fact]
    public async Task GetTasks_IsRoutedToController_NotTheStaticFileFallback()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/tasks");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskDto>>();
        Assert.NotNull(tasks);
    }
}

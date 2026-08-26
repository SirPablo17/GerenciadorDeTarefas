using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GerenciadorDeTarefas.Application.DTOs;

namespace GerenciadorDeTarefas.Tests.Integration;

public class TasksEndpointsTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
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
    public async Task Create_WithValidTitle_ReturnsCreatedTask()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/tasks", new CreateTaskRequest { Title = "Comprar pão" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var task = await response.Content.ReadFromJsonAsync<TaskDto>();
        Assert.Equal("Comprar pão", task?.Title);
    }

    [Fact]
    public async Task Create_WithEmptyTitle_ReturnsBadRequest()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/tasks", new CreateTaskRequest { Title = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task List_ReturnsOnlyOwnTasks()
    {
        var client = await CreateAuthenticatedClientAsync();
        await client.PostAsJsonAsync("/tasks", new CreateTaskRequest { Title = "Task 1" });
        await client.PostAsJsonAsync("/tasks", new CreateTaskRequest { Title = "Task 2" });

        var response = await client.GetAsync("/tasks");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskDto>>();
        Assert.Equal(2, tasks?.Count);
    }

    [Fact]
    public async Task GetById_WhenTaskExists_ReturnsTask()
    {
        var client = await CreateAuthenticatedClientAsync();
        var created = await (await client.PostAsJsonAsync("/tasks", new CreateTaskRequest { Title = "Task" }))
            .Content.ReadFromJsonAsync<TaskDto>();

        var response = await client.GetAsync($"/tasks/{created!.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WhenTaskDoesNotExist_ReturnsNotFound()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync($"/tasks/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithValidData_ReturnsUpdatedTask()
    {
        var client = await CreateAuthenticatedClientAsync();
        var created = await (await client.PostAsJsonAsync("/tasks", new CreateTaskRequest { Title = "Old title" }))
            .Content.ReadFromJsonAsync<TaskDto>();

        var response = await client.PutAsJsonAsync($"/tasks/{created!.Id}", new UpdateTaskRequest { Title = "New title" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<TaskDto>();
        Assert.Equal("New title", updated?.Title);
    }

    [Fact]
    public async Task Delete_WhenTaskExists_RemovesTaskAndSubsequentGetReturnsNotFound()
    {
        var client = await CreateAuthenticatedClientAsync();
        var created = await (await client.PostAsJsonAsync("/tasks", new CreateTaskRequest { Title = "Task" }))
            .Content.ReadFromJsonAsync<TaskDto>();

        var deleteResponse = await client.DeleteAsync($"/tasks/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync($"/tasks/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task AccessingAnotherUsersTask_ReturnsNotFound()
    {
        var ownerClient = await CreateAuthenticatedClientAsync();
        var created = await (await ownerClient.PostAsJsonAsync("/tasks", new CreateTaskRequest { Title = "Task" }))
            .Content.ReadFromJsonAsync<TaskDto>();

        var otherClient = await CreateAuthenticatedClientAsync();

        var getResponse = await otherClient.GetAsync($"/tasks/{created!.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);

        var updateResponse = await otherClient.PutAsJsonAsync($"/tasks/{created.Id}", new UpdateTaskRequest { Title = "Hacked" });
        Assert.Equal(HttpStatusCode.NotFound, updateResponse.StatusCode);

        var deleteResponse = await otherClient.DeleteAsync($"/tasks/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
    }
}

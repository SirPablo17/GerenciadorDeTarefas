using GerenciadorDeTarefas.Application.DTOs;
using GerenciadorDeTarefas.Application.Exceptions;
using GerenciadorDeTarefas.Application.Services;
using GerenciadorDeTarefas.Domain.Entities;
using GerenciadorDeTarefas.Domain.Repositories;
using Moq;

namespace GerenciadorDeTarefas.Tests.Application.Services;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _taskRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly TaskService _sut;
    private readonly Guid _userId = Guid.NewGuid();

    public TaskServiceTests()
    {
        _sut = new TaskService(_taskRepository.Object, _userRepository.Object);
    }

    [Fact]
    public async Task CreateAsync_CreatesTaskOwnedByUser()
    {
        _userRepository.Setup(r => r.GetAndIncrementNextTaskNumberAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        var request = new CreateTaskRequest
        {
            Title = "Comprar pão",
            Description = "Padaria da esquina",
            Status = TaskItemStatus.InProgress
        };

        var result = await _sut.CreateAsync(_userId, request);

        Assert.Equal("Comprar pão", result.Title);
        Assert.Equal("Padaria da esquina", result.Description);
        Assert.Equal(TaskItemStatus.InProgress, result.Status);
        _taskRepository.Verify(r => r.AddAsync(
            It.Is<TaskItem>(t => t.UserId == _userId
                && t.Title == "Comprar pão"
                && t.Description == "Padaria da esquina"
                && t.Status == TaskItemStatus.InProgress),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_AssignsNumberReturnedByUserRepository()
    {
        _userRepository.Setup(r => r.GetAndIncrementNextTaskNumberAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _sut.CreateAsync(_userId, new CreateTaskRequest { Title = "Primeira tarefa" });

        Assert.Equal(1, result.Number);
        _taskRepository.Verify(r => r.AddAsync(
            It.Is<TaskItem>(t => t.Number == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListByUserAsync_WithNoStatus_PassesNullStatusToRepositoryAndReturnsMappedTasks()
    {
        var tasks = new List<TaskItem>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Number = 1,
                Title = "Task 1",
                Description = "Description 1",
                Status = TaskItemStatus.InProgress,
                UserId = _userId,
                CreatedAt = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc)
            }
        };
        _taskRepository.Setup(r => r.ListByUserAsync(_userId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);

        var result = await _sut.ListByUserAsync(_userId);

        Assert.Single(result);
        AssertMapped(tasks[0], result[0]);
        _taskRepository.Verify(r => r.ListByUserAsync(_userId, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListByUserAsync_WithStatus_PassesStatusToRepositoryAndReturnsOnlyMatchingTasks()
    {
        var completedTasks = new List<TaskItem>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Number = 2,
                Title = "Task concluída",
                Status = TaskItemStatus.Completed,
                UserId = _userId,
                CreatedAt = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc)
            }
        };
        _taskRepository.Setup(r => r.ListByUserAsync(_userId, TaskItemStatus.Completed, It.IsAny<CancellationToken>()))
            .ReturnsAsync(completedTasks);

        var result = await _sut.ListByUserAsync(_userId, TaskItemStatus.Completed);

        Assert.Single(result);
        Assert.All(result, t => Assert.Equal(TaskItemStatus.Completed, t.Status));
        _taskRepository.Verify(r => r.ListByUserAsync(_userId, TaskItemStatus.Completed, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListByUserAsync_WhenUserHasNoTasks_ReturnsEmptyList()
    {
        _taskRepository.Setup(r => r.ListByUserAsync(_userId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TaskItem>());

        var result = await _sut.ListByUserAsync(_userId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOwnedByUser_ReturnsTask()
    {
        var taskId = Guid.NewGuid();
        var task = new TaskItem
        {
            Id = taskId,
            Title = "Task",
            Description = "Some description",
            Status = TaskItemStatus.Completed,
            UserId = _userId,
            CreatedAt = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc)
        };
        _taskRepository.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync(task);

        var result = await _sut.GetByIdAsync(_userId, taskId);

        AssertMapped(task, result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTaskDoesNotExist_ThrowsTaskNotFoundException()
    {
        var taskId = Guid.NewGuid();
        _taskRepository.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync((TaskItem?)null);

        await Assert.ThrowsAsync<TaskNotFoundException>(() => _sut.GetByIdAsync(_userId, taskId));
    }

    [Fact]
    public async Task GetByIdAsync_WhenOwnedByAnotherUser_ThrowsTaskNotFoundException()
    {
        var taskId = Guid.NewGuid();
        var otherUsersTask = new TaskItem { Id = taskId, Title = "Task", UserId = Guid.NewGuid() };
        _taskRepository.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync(otherUsersTask);

        await Assert.ThrowsAsync<TaskNotFoundException>(() => _sut.GetByIdAsync(_userId, taskId));
    }

    [Fact]
    public async Task UpdateAsync_WhenOwnedByUser_UpdatesFields()
    {
        var taskId = Guid.NewGuid();
        var originalUpdatedAt = DateTime.UtcNow.AddDays(-1);
        var task = new TaskItem
        {
            Id = taskId,
            Title = "Old title",
            Description = "Old description",
            Status = TaskItemStatus.Pending,
            UserId = _userId,
            UpdatedAt = originalUpdatedAt
        };
        _taskRepository.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync(task);

        var result = await _sut.UpdateAsync(_userId, taskId, new UpdateTaskRequest
        {
            Title = "New title",
            Description = "New description",
            Status = TaskItemStatus.Completed
        });

        Assert.Equal("New title", result.Title);
        Assert.Equal("New description", result.Description);
        Assert.Equal(TaskItemStatus.Completed, result.Status);
        Assert.True(result.UpdatedAt > originalUpdatedAt);
        _taskRepository.Verify(r => r.UpdateAsync(task, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenOwnedByAnotherUser_ThrowsTaskNotFoundException()
    {
        var taskId = Guid.NewGuid();
        var otherUsersTask = new TaskItem { Id = taskId, Title = "Task", UserId = Guid.NewGuid() };
        _taskRepository.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync(otherUsersTask);

        await Assert.ThrowsAsync<TaskNotFoundException>(() =>
            _sut.UpdateAsync(_userId, taskId, new UpdateTaskRequest { Title = "New title" }));

        _taskRepository.Verify(r => r.UpdateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenTaskDoesNotExist_ThrowsTaskNotFoundException()
    {
        var taskId = Guid.NewGuid();
        _taskRepository.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync((TaskItem?)null);

        await Assert.ThrowsAsync<TaskNotFoundException>(() =>
            _sut.UpdateAsync(_userId, taskId, new UpdateTaskRequest { Title = "New title" }));

        _taskRepository.Verify(r => r.UpdateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenOwnedByUser_DeletesTask()
    {
        var taskId = Guid.NewGuid();
        var task = new TaskItem { Id = taskId, Title = "Task", UserId = _userId };
        _taskRepository.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync(task);

        await _sut.DeleteAsync(_userId, taskId);

        _taskRepository.Verify(r => r.DeleteAsync(task, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenOwnedByAnotherUser_ThrowsTaskNotFoundException()
    {
        var taskId = Guid.NewGuid();
        var otherUsersTask = new TaskItem { Id = taskId, Title = "Task", UserId = Guid.NewGuid() };
        _taskRepository.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync(otherUsersTask);

        await Assert.ThrowsAsync<TaskNotFoundException>(() => _sut.DeleteAsync(_userId, taskId));

        _taskRepository.Verify(r => r.DeleteAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenTaskDoesNotExist_ThrowsTaskNotFoundException()
    {
        var taskId = Guid.NewGuid();
        _taskRepository.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync((TaskItem?)null);

        await Assert.ThrowsAsync<TaskNotFoundException>(() => _sut.DeleteAsync(_userId, taskId));
    }

    private static void AssertMapped(TaskItem expected, TaskDto actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Number, actual.Number);
        Assert.Equal(expected.Title, actual.Title);
        Assert.Equal(expected.Description, actual.Description);
        Assert.Equal(expected.Status, actual.Status);
        Assert.Equal(expected.CreatedAt, actual.CreatedAt);
        Assert.Equal(expected.UpdatedAt, actual.UpdatedAt);
    }
}

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
    private readonly TaskService _sut;
    private readonly Guid _userId = Guid.NewGuid();

    public TaskServiceTests()
    {
        _sut = new TaskService(_taskRepository.Object);
    }

    [Fact]
    public async Task CreateAsync_CreatesTaskOwnedByUser()
    {
        var request = new CreateTaskRequest { Title = "Comprar pão", Description = "Padaria da esquina" };

        var result = await _sut.CreateAsync(_userId, request);

        Assert.Equal("Comprar pão", result.Title);
        _taskRepository.Verify(r => r.AddAsync(
            It.Is<TaskItem>(t => t.UserId == _userId && t.Title == "Comprar pão"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListByUserAsync_ReturnsMappedTasks()
    {
        var tasks = new List<TaskItem>
        {
            new() { Id = Guid.NewGuid(), Title = "Task 1", UserId = _userId }
        };
        _taskRepository.Setup(r => r.ListByUserAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);

        var result = await _sut.ListByUserAsync(_userId);

        Assert.Single(result);
        Assert.Equal("Task 1", result[0].Title);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOwnedByUser_ReturnsTask()
    {
        var taskId = Guid.NewGuid();
        var task = new TaskItem { Id = taskId, Title = "Task", UserId = _userId };
        _taskRepository.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync(task);

        var result = await _sut.GetByIdAsync(_userId, taskId);

        Assert.Equal(taskId, result.Id);
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
        var task = new TaskItem { Id = taskId, Title = "Old title", UserId = _userId };
        _taskRepository.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync(task);

        var result = await _sut.UpdateAsync(_userId, taskId, new UpdateTaskRequest { Title = "New title", Status = TaskItemStatus.Completed });

        Assert.Equal("New title", result.Title);
        Assert.Equal(TaskItemStatus.Completed, result.Status);
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
}

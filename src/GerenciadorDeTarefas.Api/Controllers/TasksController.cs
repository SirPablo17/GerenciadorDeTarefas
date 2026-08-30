using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GerenciadorDeTarefas.Application.DTOs;
using GerenciadorDeTarefas.Application.Services;
using GerenciadorDeTarefas.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GerenciadorDeTarefas.Api.Controllers;

[ApiController]
[Authorize]
[Route("tasks")]
public class TasksController(ITaskService taskService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TaskDto>> Create(CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var task = await taskService.CreateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TaskDto>>> List([FromQuery] TaskItemStatus? status, CancellationToken cancellationToken)
    {
        var tasks = await taskService.ListByUserAsync(CurrentUserId, status, cancellationToken);
        return Ok(tasks);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var task = await taskService.GetByIdAsync(CurrentUserId, id, cancellationToken);
        return Ok(task);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TaskDto>> Update(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        var task = await taskService.UpdateAsync(CurrentUserId, id, request, cancellationToken);
        return Ok(task);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await taskService.DeleteAsync(CurrentUserId, id, cancellationToken);
        return NoContent();
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? throw new InvalidOperationException("Token não contém o claim 'sub'."));
}

using GerenciadorDeTarefas.Application.DTOs;
using GerenciadorDeTarefas.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace GerenciadorDeTarefas.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        await authService.RegisterAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await authService.LoginAsync(request, cancellationToken);
        return Ok(response);
    }
}

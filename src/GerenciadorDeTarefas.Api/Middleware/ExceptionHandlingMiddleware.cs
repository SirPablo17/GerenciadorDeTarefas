using GerenciadorDeTarefas.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace GerenciadorDeTarefas.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            var (statusCode, title) = exception switch
            {
                EmailAlreadyInUseException => (StatusCodes.Status400BadRequest, exception.Message),
                InvalidCredentialsException => (StatusCodes.Status401Unauthorized, exception.Message),
                TaskNotFoundException => (StatusCodes.Status404NotFound, exception.Message),
                _ => (StatusCodes.Status500InternalServerError, "Ocorreu um erro inesperado.")
            };

            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                logger.LogError(exception, "Erro não tratado ao processar a requisição");
            }

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Instance = context.Request.Path
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}

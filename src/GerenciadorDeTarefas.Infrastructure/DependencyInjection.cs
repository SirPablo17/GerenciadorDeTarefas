using GerenciadorDeTarefas.Application.Abstractions;
using GerenciadorDeTarefas.Domain.Repositories;
using GerenciadorDeTarefas.Infrastructure.Auth;
using GerenciadorDeTarefas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GerenciadorDeTarefas.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasherAdapter>();
        services.AddScoped<ITokenGenerator, JwtTokenGenerator>();

        return services;
    }
}

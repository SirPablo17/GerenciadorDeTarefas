using GerenciadorDeTarefas.Domain.Entities;
using GerenciadorDeTarefas.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorDeTarefas.Infrastructure.Persistence;

public class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetAndIncrementNextTaskNumberAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var assignedNumbers = await dbContext.Database
            .SqlQuery<int>($"""
                UPDATE Users SET NextTaskNumber = NextTaskNumber + 1
                WHERE Id = {userId}
                RETURNING NextTaskNumber - 1 AS Value
                """)
            .ToListAsync(cancellationToken);

        return assignedNumbers.Single();
    }
}

using EmployeeChallenge.Api.Core.Entities;
using EmployeeChallenge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EmployeeChallenge.Api.Core.Repositories;

internal interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}

internal class UserRepository(IUnitOfWork unitOfWork) : Repository<User>(unitOfWork), IUserRepository
{
    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(u => u.Username == username, cancellationToken).ConfigureAwait(false);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(u => u.Email == email, cancellationToken).ConfigureAwait(false);
    }
}

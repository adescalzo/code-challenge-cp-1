using Microsoft.EntityFrameworkCore;

namespace EmployeeChallenge.Infrastructure.Data;

public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    DbContext Context { get; }
    bool HasPendingChanges();
}

public sealed class UnitOfWork(DbContext context) : IUnitOfWork
{
    public DbContext Context { get; } = context;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return result;
    }

    public bool HasPendingChanges()
    {
        return Context.ChangeTracker.HasChanges();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~UnitOfWork()
    {
        Dispose(false);
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            Context.Dispose();
        }
    }
}

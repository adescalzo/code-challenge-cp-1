using System.Collections.Concurrent;
using EmployeeChallenge.Api.Core.Entities;
using EmployeeChallenge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EmployeeChallenge.Api.Core.Repositories;

internal interface IEmployeeRepository : IRepository<Employee>
{
    Task<Employee?> GetByIdWithSupervisorAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Employee>> GetPaginatedWithSupervisorAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetTotalReportsCountAsync(Guid supervisorId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Employee>> GetDirectReportsAsync(Guid supervisorId, CancellationToken cancellationToken = default);
}

internal class EmployeeRepository(IUnitOfWork unitOfWork) : Repository<Employee>(unitOfWork), IEmployeeRepository
{
    public async Task<Employee?> GetByIdWithSupervisorAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.Supervisor)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IEnumerable<Employee>> GetPaginatedWithSupervisorAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await GetPaginatedQueryable(page, pageSize, tracking: false)
            .Include(e => e.Supervisor)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<int> GetTotalReportsCountAsync(Guid supervisorId, CancellationToken cancellationToken = default)
    {
        var allReports = new ConcurrentBag<Guid>();

        await GetAllReportsRecursive(supervisorId, allReports, cancellationToken).ConfigureAwait(false);

        return allReports.Distinct().Count();
    }

    public async Task<IEnumerable<Employee>> GetDirectReportsAsync(
        Guid supervisorId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e => e.SupervisorId == supervisorId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task GetAllReportsRecursive(
        Guid supervisorId,
        ConcurrentBag<Guid> allReports,
        CancellationToken cancellationToken)
    {
        var directReports = await DbSet
            .Where(e => e.SupervisorId == supervisorId)
            .Select(e => new { e.Id, e.IsSupervisor })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var report in directReports)
        {
            allReports.Add(report.Id);
        }

        var supervisorIds = directReports
            .Where(x => x.IsSupervisor)
            .Select(x => x.Id)
            .ToList();

        if (supervisorIds.Count > 0)
        {
            var tasks = supervisorIds.Select(id =>
                GetAllReportsRecursive(id, allReports, cancellationToken)
            );
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}

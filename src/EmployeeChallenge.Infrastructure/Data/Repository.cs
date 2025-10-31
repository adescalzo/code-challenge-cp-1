using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace EmployeeChallenge.Infrastructure.Data;

/// <summary>
/// Generic repository interface for data access operations
/// </summary>
/// <typeparam name="TEntity">The entity type that implements IDocument</typeparam>
public interface IRepository<TEntity> where TEntity : class, IDocument
{
    /// <summary>
    /// Gets the unit of work associated with this repository
    /// </summary>
    IUnitOfWork UnitOfWork { get; }

    /// <summary>
    /// Retrieves an entity by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the entity</param>
    /// <param name="tracking">Whether to track the entity in the context. Default is true</param>
    /// <returns>The entity if found, otherwise null</returns>
    Task<TEntity?> GetById(Guid id, bool tracking = true);

    /// <summary>
    /// Retrieves an entity based on a predicate expression
    /// </summary>
    /// <param name="predicate">Expression to filter entities</param>
    /// <param name="tracking">Whether to track the entity in the context. Default is true</param>
    /// <returns>The first entity matching the predicate, or null if not found</returns>
    Task<TEntity?> GetByAsync(Expression<Func<TEntity, bool>> predicate, bool tracking = true);

    /// <summary>
    /// Gets a queryable collection of entities allowing for additional LINQ operations like Include, Where, etc.
    /// </summary>
    /// <param name="tracking">Whether to track entities in the context. Default is true</param>
    /// <returns>IQueryable of entities for further composition</returns>
    IQueryable<TEntity> GetQueryable(bool tracking = true);

    /// <summary>
    /// Gets a paginated queryable collection of entities with Skip and Take already applied.
    /// Allows for additional operations like Include to be chained before execution
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="tracking">Whether to track entities in the context. Default is true</param>
    /// <returns>IQueryable with pagination applied for further composition</returns>
    IQueryable<TEntity> GetPaginatedQueryable(int page, int pageSize, bool tracking = true);

    /// <summary>
    /// Retrieves all entities from the repository
    /// </summary>
    /// <param name="tracking">Whether to track entities in the context. Default is true</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of all entities</returns>
    Task<IEnumerable<TEntity>> GetAllAsync(bool tracking = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all entities with a projection to transform the result
    /// </summary>
    /// <typeparam name="T">The projection result type</typeparam>
    /// <param name="projection">Expression to project entities to desired shape</param>
    /// <param name="tracking">Whether to track entities in the context. Default is true</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of projected results</returns>
    Task<IEnumerable<T>> GetAllAsync<T>(
        Expression<Func<TEntity, T>> projection,
        bool tracking = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds entities that match the specified predicate
    /// </summary>
    /// <param name="predicate">Expression to filter entities</param>
    /// <param name="tracking">Whether to track entities in the context. Default is true</param>
    /// <returns>Collection of matching entities</returns>
    Task<IEnumerable<TEntity>> Find(Expression<Func<TEntity, bool>> predicate, bool tracking = true);

    /// <summary>
    /// Finds entities matching a predicate and projects them to a specified type
    /// </summary>
    /// <typeparam name="T">The projection result type</typeparam>
    /// <param name="predicate">Expression to filter entities</param>
    /// <param name="projection">Expression to project entities to desired shape</param>
    /// <param name="tracking">Whether to track entities in the context. Default is true</param>
    /// <returns>Collection of projected results matching the predicate</returns>
    Task<IEnumerable<T>> Find<T>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, T>> projection,
        bool tracking = true
    );

    /// <summary>
    /// Returns the first entity matching the predicate, or null if no match is found
    /// </summary>
    /// <param name="predicate">Expression to filter entities</param>
    /// <param name="tracking">Whether to track the entity in the context. Default is true</param>
    /// <returns>The first matching entity or null</returns>
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, bool tracking = true);

    /// <summary>
    /// Determines whether any entity matches the specified predicate
    /// </summary>
    /// <param name="predicate">Expression to test entities against</param>
    /// <returns>True if any entity matches the predicate, otherwise false</returns>
    Task<bool> Any(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Adds a new entity to the repository
    /// </summary>
    /// <param name="entity">The entity to add</param>
    /// <returns>The added entity</returns>
    Task<TEntity> Add(TEntity entity);

    /// <summary>
    /// Removes an entity from the repository
    /// </summary>
    /// <param name="entity">The entity to remove</param>
    /// <returns>The removed entity</returns>
    TEntity Remove(TEntity entity);

    /// <summary>
    /// Updates an existing entity in the repository
    /// </summary>
    /// <param name="entity">The entity to update</param>
    /// <returns>The updated entity</returns>
    TEntity Update(TEntity entity);

    /// <summary>
    /// Updates multiple entities in the repository
    /// </summary>
    /// <param name="entities">The collection of entities to update</param>
    void Update(IEnumerable<TEntity> entities);

    /// <summary>
    /// Executes an update operation directly in the database using ExecuteUpdate
    /// </summary>
    /// <param name="setPropertyCalls">Expression defining which properties to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of entities affected</returns>
    Task<int> ExecuteUpdate(
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls,
        CancellationToken cancellationToken = default
    );
}

public class Repository<TEntity>(IUnitOfWork unitOfWork) : IRepository<TEntity>
    where TEntity : class, IDocument
{
    private readonly DbContext _context = unitOfWork.Context;

    public IUnitOfWork UnitOfWork => unitOfWork;

    public async Task<TEntity?> GetById(Guid id, bool tracking = true)
    {
        var e = _context.Set<TEntity>();
        var result = !tracking
            ? await e.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id).ConfigureAwait(false)
            : await e.FirstOrDefaultAsync(x => x.Id == id).ConfigureAwait(false);

        return result;
    }

    public IQueryable<TEntity> GetQueryable(bool tracking = true)
    {
        var q = _context.Set<TEntity>().AsQueryable();
        if (!tracking)
        {
            q = q.AsNoTracking();
        }

        return q;
    }

    public IQueryable<TEntity> GetPaginatedQueryable(int page, int pageSize, bool tracking = true)
    {
        var skip = (page - 1) * pageSize;

        var query = GetQueryable(tracking)
            .Skip(skip)
            .Take(pageSize);

        return query;
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(
        bool tracking = true,
        CancellationToken cancellationToken = default)
    {
        var e = _context.Set<TEntity>();
        if (!tracking)
        {
            return await e.AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        return await e.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IEnumerable<T>> GetAllAsync<T>(
        Expression<Func<TEntity, T>> projection,
        bool tracking = true,
        CancellationToken cancellationToken = default)
    {
        var e = _context.Set<TEntity>();
        if (!tracking)
        {
            return await e.AsNoTracking().Select(projection).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        return await e.Select(projection).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public DbSet<TEntity> DbSet => _context.Set<TEntity>();

    public async Task<IEnumerable<TEntity>> Find(Expression<Func<TEntity, bool>> predicate, bool tracking = true)
    {
        var e = _context.Set<TEntity>().Where(predicate);
        if (!tracking)
        {
            e = e.AsNoTracking();
        }

        return await e.ToListAsync().ConfigureAwait(false);
    }

    public async Task<IEnumerable<T>> Find<T>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, T>> projection,
        bool tracking = true
    )
    {
        var e = _context.Set<TEntity>().Where(predicate);
        if (!tracking)
        {
            e = e.AsNoTracking();
        }

        return await e.Select(projection).ToListAsync().ConfigureAwait(false);
    }

    public async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool tracking = true)
    {
        var e = _context.Set<TEntity>().Where(predicate);
        if (!tracking)
        {
            e = e.AsNoTracking();
        }

        return await e.FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<TEntity?> GetByAsync(Expression<Func<TEntity, bool>> predicate, bool tracking = true)
    {
        var e = _context.Set<TEntity>().Where(predicate);
        if (!tracking)
        {
            e = e.AsNoTracking();
        }

        return await e.FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<bool> Any(Expression<Func<TEntity, bool>> predicate)
    {
        return await _context.Set<TEntity>().AsNoTracking().AnyAsync(predicate).ConfigureAwait(false);
    }

    public async Task<TEntity> Add(TEntity entity)
    {
        var e = await _context.Set<TEntity>().AddAsync(entity).ConfigureAwait(false);
        return e.Entity;
    }

    public TEntity Remove(TEntity entity)
    {
        var r = _context.Set<TEntity>().Remove(entity);
        return r.Entity;
    }

    public TEntity Update(TEntity entity)
    {
        var e = _context.Set<TEntity>().Update(entity);
        return e.Entity;
    }

    public void Update(IEnumerable<TEntity> entities) => _context.Set<TEntity>().UpdateRange(entities);

    public async Task<int> ExecuteUpdate(
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls,
        CancellationToken cancellationToken = default
    )
    {
        var r =
            await _context.Set<TEntity>().ExecuteUpdateAsync(setPropertyCalls, cancellationToken).ConfigureAwait(false);
        return r;
    }
}

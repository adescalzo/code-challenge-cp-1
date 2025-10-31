using EmployeeChallenge.Api.Core;
using EmployeeChallenge.Infrastructure;
using EmployeeChallenge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace EmployeeChallenge.Api.Tests.IntegrationTests;

public abstract class AsyncLifetimeBase : IAsyncLifetime, IDisposable
{
    private ApplicationDbContext? _context;
    private bool _disposed;
    private UnitOfWork? _unitOfWork;

    protected AsyncLifetimeBase(ITestOutputHelper testOutputHelper)
    {
        OutputHelper = testOutputHelper;
        PreInitialization();
    }

    public ITestOutputHelper OutputHelper { get; }

    protected DbContext Context => _context ?? throw new InvalidOperationException("Context not initialized");

    public IConfigurationRoot ConfigurationRoot { get; private set; } = default!;

    public IClock Clock { get; protected set; } = new Clock();

    public IUnitOfWork CreateUnitOfWork()
    {
        if (_unitOfWork != null)
        {
            return _unitOfWork;
        }

        _unitOfWork = new UnitOfWork(Context);
        return _unitOfWork;
    }

    public async Task InitializeAsync()
    {
        CreateContext();
        await OnInitializeAsync().ConfigureAwait(false);
    }

    public async Task DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        await OnDisposeAsync().ConfigureAwait(false);

        if (_context != null)
        {
            await _context.DisposeAsync().ConfigureAwait(false);
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            OnDisposeAsync().GetAwaiter().GetResult();
            _context?.Dispose();
            _unitOfWork?.Dispose();
        }

        _disposed = true;
    }

    protected virtual Task OnInitializeAsync() => Task.CompletedTask;

    protected virtual Task OnDisposeAsync() => Task.CompletedTask;

    protected async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private void CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"EmployeeChallengeTestDb_{Guid.NewGuid()}")
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
            .Options;

        _context = new ApplicationDbContext(Clock, options);
    }

    private void PreInitialization()
    {
        var builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.tests.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddUserSecrets<AsyncLifetimeBase>();

        ConfigurationRoot = builder.Build();
    }
}

using EmployeeChallenge.Infrastructure.General;
using EmployeeChallenge.Api.Tests.Testing;
using NSubstitute;

namespace EmployeeChallenge.Api.Tests.Mocks;

/// <summary>
/// Mock wrapper for IClock that allows configuring specific UTC time and today values.
/// </summary>
internal class MockClock
{
    private readonly FluentSubstitute<IClock> _substitute;

    public IClock Instance => _substitute.Instance;

    public MockClock()
    {
        _substitute = FluentSubstitute.For<IClock>();
    }

    /// <summary>
    /// Configure the UtcNow property to return a specific DateTime.
    /// </summary>
    public MockClock WithUtcNow(DateTime utcNow)
    {
        _substitute.Configure(c => c.UtcNow.Returns(utcNow));
        return this;
    }

    /// <summary>
    /// Configure the Today property to return a specific DateTime.
    /// </summary>
    public MockClock WithToday(DateTime today)
    {
        _substitute.Configure(c => c.Today.Returns(today));
        return this;
    }

    /// <summary>
    /// Get the configured UtcNow value (for assertions/comparisons).
    /// </summary>
    public DateTime GetUtcNow() => Instance.UtcNow;
}


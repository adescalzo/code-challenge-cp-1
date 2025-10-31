using System;
using NSubstitute;

namespace EmployeeChallenge.Api.Tests.Testing;

/// <summary>
/// A small fluent wrapper around NSubstitute substitutes that supports chaining
/// and conditional configuration. Use it when you want Moq-like wrapper ergonomics
/// but with NSubstitute semantics.
///
/// Example:
/// var mock = FluentSubstitute.For&lt;IMyService&gt;()
///     .Configure(s => s.Get(Arg.Any&lt;int&gt;()).Returns("ok"))
///     .ConfigureIf(shouldMockExtra, s => s.Extra(Arg.Any&lt;string&gt;()).Returns(true));
/// IMyService instance = mock.Instance; // access Instance property
/// </summary>
public class FluentSubstitute<T> where T : class
{
    public T Instance { get; }

    public FluentSubstitute()
    {
        Instance = Substitute.For<T>();
    }

    internal FluentSubstitute(T instance)
    {
        Instance = instance;
    }

    /// <summary>
    /// Apply a configuration action to the underlying substitute and return this for chaining.
    /// </summary>
    public FluentSubstitute<T> Configure(Action<T> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(Instance);

        return this;
    }

    /// <summary>
    /// Apply the configuration only if the provided condition is true. Returns this for chaining.
    /// </summary>
    public FluentSubstitute<T> ConfigureIf(bool condition, Action<T> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        if (condition)
        {
            configure(Instance);
        }

        return this;
    }

    /// <summary>
    /// Apply the default configuration. Provided as a named helper to make intent clearer.
    /// </summary>
    public FluentSubstitute<T> WithDefaults(Action<T> defaults)
    {
        ArgumentNullException.ThrowIfNull(defaults);

        defaults(Instance);

        return this;
    }

    /// <summary>
    /// Explicit accessor to get the underlying substitute instance.
    /// Use this instead of an implicit conversion to satisfy analyzers and keep intent explicit.
    /// </summary>
    public T ToInstance() => Instance;
}

/// <summary>
/// Non-generic factory helpers for creating FluentSubstitute wrappers.
/// Keeps the generic type free of static members to satisfy analyzer rules.
/// </summary>
public static class FluentSubstitute
{
    public static FluentSubstitute<T> For<T>() where T : class => new FluentSubstitute<T>();

    public static FluentSubstitute<T> ForParts<T>(params object[] constructorArgs) where T : class
    {
        var inst = Substitute.ForPartsOf<T>(constructorArgs);
        return new FluentSubstitute<T>(inst);
    }
}

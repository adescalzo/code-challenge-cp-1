using Bogus;
using EmployeeChallenge.Api.Tests.Testing;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace EmployeeChallenge.Api.Tests.Mocks;

/// <summary>
/// Mock wrapper for JWT Settings configuration (IConfiguration with JwtSettings section).
/// Provides a fluent API to configure all JWT-related settings: SecretKey, Issuer, Audience, ExpirationMinutes.
/// </summary>
internal class MockJwtSettings
{
    private readonly FluentSubstitute<IConfiguration> _configSubstitute;
    private readonly FluentSubstitute<IConfigurationSection> _sectionSubstitute;
    private readonly Faker _faker = new();

    public IConfiguration Configuration => _configSubstitute.Instance;
    public IConfigurationSection JwtSection => _sectionSubstitute.Instance;

    public MockJwtSettings()
    {
        _sectionSubstitute = FluentSubstitute.For<IConfigurationSection>();
        _configSubstitute = FluentSubstitute.For<IConfiguration>();

        // Default: wire JwtSettings section into configuration
        _configSubstitute.Configure(c => c.GetSection("JwtSettings").Returns(_sectionSubstitute.Instance));

        // Default: generate random valid JWT settings
        SetupDefaults();
    }

    /// <summary>
    /// Set up default JWT settings with random values (for tests that don't care about specific values).
    /// </summary>
    private void SetupDefaults()
    {
        WithSecretKey(_faker.Random.String(length: 50, minChar: '!', maxChar: '~'));
        WithIssuer(_faker.Company.CompanyName());
        WithAudience(_faker.Company.CompanyName());
        WithExpirationMinutes(_faker.Random.Number(5, 60));
    }

    /// <summary>
    /// Configure the JWT SecretKey.
    /// </summary>
    public MockJwtSettings WithSecretKey(string? secretKey)
    {
        _sectionSubstitute.Configure(s => s["SecretKey"].Returns(secretKey));
        return this;
    }

    /// <summary>
    /// Configure the JWT Issuer.
    /// </summary>
    public MockJwtSettings WithIssuer(string? issuer)
    {
        _sectionSubstitute.Configure(s => s["Issuer"].Returns(issuer));
        return this;
    }

    /// <summary>
    /// Configure the JWT Audience.
    /// </summary>
    public MockJwtSettings WithAudience(string? audience)
    {
        _sectionSubstitute.Configure(s => s["Audience"].Returns(audience));
        return this;
    }

    /// <summary>
    /// Configure the JWT ExpirationMinutes.
    /// </summary>
    public MockJwtSettings WithExpirationMinutes(int minutes)
    {
        _sectionSubstitute.Configure(s => s["ExpirationMinutes"].Returns(minutes.ToString()));
        return this;
    }

    /// <summary>
    /// Configure the JWT ExpirationMinutes as a string.
    /// </summary>
    public MockJwtSettings WithExpirationMinutesString(string? minutes)
    {
        _sectionSubstitute.Configure(s => s["ExpirationMinutes"].Returns(minutes));
        return this;
    }

    /// <summary>
    /// Get the configured SecretKey.
    /// </summary>
    public string? GetSecretKey() => JwtSection["SecretKey"];

    /// <summary>
    /// Get the configured Issuer.
    /// </summary>
    public string? GetIssuer() => JwtSection["Issuer"];

    /// <summary>
    /// Get the configured Audience.
    /// </summary>
    public string? GetAudience() => JwtSection["Audience"];

    /// <summary>
    /// Get the configured ExpirationMinutes as an int.
    /// </summary>
    public int GetExpirationMinutes()
    {
        var value = JwtSection["ExpirationMinutes"];
        _ = int.TryParse(value ?? "60", out var result);

        return result;
    }
}


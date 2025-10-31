using EmployeeChallenge.Api.Application.Services;
using EmployeeChallenge.Api.Core.Entities;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using System.IdentityModel.Tokens.Jwt;
using EmployeeChallenge.Infrastructure;

namespace EmployeeChallenge.Api.Tests.Application.Services;

public class AuthServiceTests
{
    private readonly IConfiguration _configuration;
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _configuration = Substitute.For<IConfiguration>();
        var jwtSection = Substitute.For<IConfigurationSection>();

        _configuration.GetSection("JwtSettings").Returns(jwtSection);
        jwtSection["SecretKey"].Returns("ThisIsAVerySecureSecretKeyForTestingPurposesOnly12345");
        jwtSection["Issuer"].Returns("TestIssuer");
        jwtSection["Audience"].Returns("TestAudience");
        jwtSection["ExpirationMinutes"].Returns("60");

        _sut = new AuthService(_clock, _configuration);
    }

    [Fact]
    public void GenerateJwtToken_ShouldReturnValidToken()
    {
        // Arrange
        var user = new User(
            Guid.NewGuid(),
            "testuser",
            "test@example.com",
            "hashedpassword",
            "John",
            "Doe"
        );

        // Act
        var token = _sut.GenerateJwtToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Issuer.Should().Be("TestIssuer");
        jwtToken.Audiences.Should().Contain("TestAudience");
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id.ToString());
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.UniqueName && c.Value == user.Username);
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
        jwtToken.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(60), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateJwtToken_WhenSecretKeyMissing_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var config = Substitute.For<IConfiguration>();
        var jwtSection = Substitute.For<IConfigurationSection>();
        config.GetSection("JwtSettings").Returns(jwtSection);
        jwtSection["SecretKey"].Returns((string?)null);

        var authService = new AuthService(_clock, config);
        var user = new User(Guid.NewGuid(), "testuser", "test@example.com", "password", "John", "Doe");

        // Act
        var act = () => authService.GenerateJwtToken(user);

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("JWT SecretKey not configured");
    }

    [Fact]
    public void GenerateJwtToken_WhenIssuerMissing_ShouldUseDefaultValue()
    {
        // Arrange
        var config = Substitute.For<IConfiguration>();
        var jwtSection = Substitute.For<IConfigurationSection>();
        config.GetSection("JwtSettings").Returns(jwtSection);
        jwtSection["SecretKey"].Returns("ThisIsAVerySecureSecretKeyForTestingPurposesOnly12345");
        jwtSection["Issuer"].Returns((string?)null);
        jwtSection["Audience"].Returns("TestAudience");
        jwtSection["ExpirationMinutes"].Returns("60");

        var authService = new AuthService(_clock, config);
        var user = new User(Guid.NewGuid(), "testuser", "test@example.com", "password", "John", "Doe");

        // Act
        var token = authService.GenerateJwtToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        jwtToken.Issuer.Should().Be("EmployeeChallenge");
    }

    [Fact]
    public void HashPassword_ShouldReturnBase64Hash()
    {
        // Arrange
        const string password = "MySecurePassword123!";

        // Act
        var hash = _sut.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().MatchRegex(@"^[A-Za-z0-9+/=]+$"); // Base64 pattern
    }

    [Fact]
    public void HashPassword_ShouldReturnConsistentHash()
    {
        // Arrange
        const string password = "MySecurePassword123!";

        // Act
        var hash1 = _sut.HashPassword(password);
        var hash2 = _sut.HashPassword(password);

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        const string password = "MySecurePassword123!";
        var hash = _sut.HashPassword(password);

        // Act
        var result = _sut.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        const string correctPassword = "MySecurePassword123!";
        const string incorrectPassword = "WrongPassword";
        var hash = _sut.HashPassword(correctPassword);

        // Act
        var result = _sut.VerifyPassword(incorrectPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_CaseSensitive_ShouldReturnFalse()
    {
        // Arrange
        const string password = "MySecurePassword";
        var hash = _sut.HashPassword(password);

        // Act
        var result = _sut.VerifyPassword("mysecurepassword", hash);

        // Assert
        result.Should().BeFalse();
    }
}

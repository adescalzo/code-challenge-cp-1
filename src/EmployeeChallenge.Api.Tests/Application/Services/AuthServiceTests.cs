using System.IdentityModel.Tokens.Jwt;
using Bogus;
using EmployeeChallenge.Api.Application.Services;
using EmployeeChallenge.Api.Tests.Builders;
using EmployeeChallenge.Api.Tests.Mocks;
using FluentAssertions;

namespace EmployeeChallenge.Api.Tests.Application.Services;

public class AuthServiceTests
{
    private readonly Faker _faker = new();
    [Fact]
    public void GenerateJwtToken_ShouldReturnValidToken()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var jwtSettings = new MockJwtSettings();
        var clock = new MockClock().WithUtcNow(now);
        var encryption = new MockEncryptionService();
        var authService = new AuthService(clock.Instance, encryption.Instance, jwtSettings.Configuration);

        var user = new UserBuilder()
            .WithUsername("testuser")
            .WithEmail(_faker.Internet.Email())
            .WithName("John", "Doe")
            .Build();

        // Act
        var token = authService.GenerateJwtToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Issuer.Should().Be(jwtSettings.GetIssuer());
        jwtToken.Audiences.Should().Contain(jwtSettings.GetAudience());
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id.ToString());
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.UniqueName && c.Value == user.Username);
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
        var expirationMinutes = jwtSettings.GetExpirationMinutes();
        jwtToken.ValidTo.Should().Be(now.AddMinutes(expirationMinutes));
    }

    [Fact]
    public void GenerateJwtToken_WhenSecretKeyMissing_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var jwtSettings = new MockJwtSettings().WithSecretKey(null);
        var clock = new MockClock();
        var encryption = new MockEncryptionService();
        var authService = new AuthService(clock.Instance, encryption.Instance, jwtSettings.Configuration);

        var user = new UserBuilder()
            .WithUsername("testuser")
            .WithEmail("test@example.com")
            .Build();

        // Act
        var act = () => authService.GenerateJwtToken(user);

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("JWT SecretKey not configured");
    }

    [Fact]
    public void GenerateJwtToken_WhenIssuerMissing_ShouldUseDefaultValue()
    {
        // Arrange
        var jwtSettings = new MockJwtSettings()
            .WithSecretKey("ThisIsAVerySecureSecretKeyForTestingPurposesOnly12345")
            .WithIssuer(null)
            .WithAudience("TestAudience")
            .WithExpirationMinutes(60);

        var clock = new MockClock();
        var encryption = new MockEncryptionService();
        var authService = new AuthService(clock.Instance, encryption.Instance, jwtSettings.Configuration);

        var user = new UserBuilder()
            .WithUsername("testuser")
            .WithEmail("test@example.com")
            .Build();

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
        var jwtSettings = new MockJwtSettings();
        var clock = new MockClock();
        var encryption = new MockEncryptionService();
        var authService = new AuthService(clock.Instance, encryption.Instance, jwtSettings.Configuration);

        const string password = "MySecurePassword123!";

        // Act
        var hash = authService.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().MatchRegex(@"^[A-Za-z0-9+/=]+$"); // Base64 pattern
    }

    [Fact]
    public void HashPassword_ShouldReturnConsistentHash()
    {
        // Arrange
        var jwtSettings = new MockJwtSettings();
        var clock = new MockClock();
        var encryption = new MockEncryptionService();
        var authService = new AuthService(clock.Instance, encryption.Instance, jwtSettings.Configuration);

        const string password = "MySecurePassword123!";

        // Act
        var hash1 = authService.HashPassword(password);
        var hash2 = authService.HashPassword(password);

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var jwtSettings = new MockJwtSettings();
        var clock = new MockClock();
        var encryption = new MockEncryptionService();
        var authService = new AuthService(clock.Instance, encryption.Instance, jwtSettings.Configuration);

        const string password = "MySecurePassword123!";
        var hash = authService.HashPassword(password);

        // Act
        var result = authService.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var jwtSettings = new MockJwtSettings();
        var clock = new MockClock();
        var encryption = new MockEncryptionService();
        var authService = new AuthService(clock.Instance, encryption.Instance, jwtSettings.Configuration);

        const string correctPassword = "MySecurePassword123!";
        const string incorrectPassword = "WrongPassword";
        var hash = authService.HashPassword(correctPassword);

        // Act
        var result = authService.VerifyPassword(incorrectPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_CaseSensitive_ShouldReturnFalse()
    {
        // Arrange
        var jwtSettings = new MockJwtSettings();
        var clock = new MockClock();
        var encryption = new MockEncryptionService();
        var authService = new AuthService(clock.Instance, encryption.Instance, jwtSettings.Configuration);

        const string password = "MySecurePassword";
        var hash = authService.HashPassword(password);

        // Act
        var result = authService.VerifyPassword("mysecurepassword", hash);

        // Assert
        result.Should().BeFalse();
    }
}

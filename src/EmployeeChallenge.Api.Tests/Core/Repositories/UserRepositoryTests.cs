using EmployeeChallenge.Api.Core.Repositories;
using EmployeeChallenge.Api.Tests.Builders;
using EmployeeChallenge.Api.Tests.IntegrationTests;
using FluentAssertions;
using Xunit.Abstractions;

namespace EmployeeChallenge.Api.Tests.Core.Repositories;

public class UserRepositoryTests(ITestOutputHelper output) : AsyncLifetimeBase(output)
{
    private UserRepository _repository = null!;

    protected override async Task OnInitializeAsync()
    {
        await Context.Database.EnsureCreatedAsync().ConfigureAwait(false);
        _repository = new UserRepository(CreateUnitOfWork());
    }

    protected override async Task OnDisposeAsync()
    {
        await Context.Database.EnsureDeletedAsync().ConfigureAwait(false);
    }

    [Fact]
    public async Task GetByUsernameAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var user = new UserBuilder()
            .WithUsername("testuser")
            .Build();

        await _repository.Add(user);
        await SaveChangesAsync();

        // Act
        var result = await _repository.GetByUsernameAsync("testuser");

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be("testuser");
        result.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetByUsernameAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByUsernameAsync("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var user = new UserBuilder()
            .WithEmail("test@example.com")
            .Build();

        await _repository.Add(user);
        await SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync("test@example.com");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
        result.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetByEmailAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByEmailAsync("nonexistent@example.com");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUsernameAsync_WithMultipleUsers_ShouldReturnCorrectUser()
    {
        // Arrange
        var users = new UserBuilder().BuildList(5).ToList();
        foreach (var user in users)
        {
            await _repository.Add(user);
        }
        await SaveChangesAsync();

        var targetUser = users[2];

        // Act
        var result = await _repository.GetByUsernameAsync(targetUser.Username);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(targetUser.Id);
        result.Username.Should().Be(targetUser.Username);
    }

    [Fact]
    public async Task GetByEmailAsync_WithMultipleUsers_ShouldReturnCorrectUser()
    {
        // Arrange
        var users = new UserBuilder().BuildList(3).ToList();
        foreach (var user in users)
        {
            await _repository.Add(user);
        }
        await SaveChangesAsync();

        var targetUser = users[1];

        // Act
        var result = await _repository.GetByEmailAsync(targetUser.Email);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(targetUser.Id);
        result.Email.Should().Be(targetUser.Email);
    }
}

using Bogus;
using EmployeeChallenge.Api.Core.Entities;
using EmployeeChallenge.Infrastructure.Testing;

namespace EmployeeChallenge.Api.Tests.Builders;

internal class UserBuilder : IBuilder<User>
{
    private Guid? _id;
    private string? _username;
    private string? _email;
    private string? _password;
    private string? _firstName;
    private string? _lastName;

    public UserBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public UserBuilder WithUsername(string username)
    {
        _username = username;
        return this;
    }

    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public UserBuilder WithPassword(string password)
    {
        _password = password;
        return this;
    }

    public UserBuilder WithName(string firstName, string lastName)
    {
        _firstName = firstName;
        _lastName = lastName;
        return this;
    }

    public User Build()
    {
        var faker = new Faker();
        return new User(
            _id ?? Guid.NewGuid(),
            _username ?? faker.Internet.UserName(),
            _email ?? faker.Internet.Email(),
            _password ?? faker.Internet.Password(8),
            _firstName ?? faker.Name.FirstName(),
            _lastName ?? faker.Name.LastName()
        );
    }

    public IEnumerable<User> BuildList(int count)
    {
        var users = new List<User>();
        for (var i = 0; i < count; i++)
        {
            var faker = new Faker();
            users.Add(new User(
                _id ?? Guid.NewGuid(),
                _username ?? faker.Internet.UserName(),
                _email ?? faker.Internet.Email(),
                _password ?? faker.Internet.Password(8),
                _firstName ?? faker.Name.FirstName(),
                _lastName ?? faker.Name.LastName()
            ));
        }
        return users;
    }
}

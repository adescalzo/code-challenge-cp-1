using System.Text;
using EmployeeChallenge.Infrastructure.General;
using EmployeeChallenge.Api.Tests.Testing;
using NSubstitute;

namespace EmployeeChallenge.Api.Tests.Mocks;

/// <summary>
/// Mock wrapper for IEncryptionService.
/// By default, uses a simple Base64 encoding for testing (not cryptographically secure).
/// Can be configured to use different encryption strategies.
/// </summary>
internal class MockEncryptionService
{
    private readonly FluentSubstitute<IEncryptionService> _substitute;

    public IEncryptionService Instance => _substitute.Instance;

    public MockEncryptionService()
    {
        _substitute = FluentSubstitute.For<IEncryptionService>();
        // Default: simple Base64 encoding for predictable testing
        SetupDefaultEncryption();
    }

    /// <summary>
    /// Configure with the default Base64 encryption strategy.
    /// PasswordEncrypt returns Base64 of the input.
    /// VerifyPassword compares Base64(entered) with stored hash.
    /// </summary>
    public MockEncryptionService WithDefaultEncryption()
    {
        SetupDefaultEncryption();
        return this;
    }

    /// <summary>
    /// Configure with a custom PasswordEncrypt implementation.
    /// </summary>
    public MockEncryptionService WithPasswordEncrypt(Func<string, string> encryptFunc)
    {
        _substitute.Configure(s => s.PasswordEncrypt(Arg.Any<string>())
            .Returns(x => encryptFunc(x.Arg<string>())));
        return this;
    }

    /// <summary>
    /// Configure with a custom VerifyPassword implementation.
    /// </summary>
    public MockEncryptionService WithVerifyPassword(Func<string, string, bool> verifyFunc)
    {
        _substitute.Configure(s => s.VerifyPassword(Arg.Any<string>(), Arg.Any<string>())
            .Returns(x => verifyFunc(x.Arg<string>(), x.ArgAt<string>(1))));
        return this;
    }

    private void SetupDefaultEncryption()
    {
        _substitute.Configure(s => s.PasswordEncrypt(Arg.Any<string>())
            .Returns(x => Convert.ToBase64String(Encoding.UTF8.GetBytes(x.Arg<string>()))));

        _substitute.Configure(s => s.VerifyPassword(Arg.Any<string>(), Arg.Any<string>())
            .Returns(x =>
            {
                var entered = x.Arg<string>();
                var stored = x.ArgAt<string>(1);
                var enteredHash = Convert.ToBase64String(Encoding.UTF8.GetBytes(entered));
                return enteredHash == stored;
            }));
    }
}


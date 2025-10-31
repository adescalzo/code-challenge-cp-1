using Xunit.Abstractions;

namespace EmployeeChallenge.Api.Tests.IntegrationTests;

[CollectionDefinition(nameof(TestLocalFixture))]
public class TestLocalCollectionFixture : ICollectionFixture<TestLocalFixture>;

public class TestLocalFixture(ITestOutputHelper outputHelper) : AsyncLifetimeBase(outputHelper)
{
    // Context is automatically created in base class InitializeAsync
    // Context is automatically disposed in base class DisposeAsync
}

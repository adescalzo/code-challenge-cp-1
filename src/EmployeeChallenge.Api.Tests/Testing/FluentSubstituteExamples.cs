using NSubstitute;

namespace EmployeeChallenge.Api.Tests.Testing;

// Example usages for the FluentSubstitute wrapper.
internal static class FluentSubstituteExamples
{
    // Example interface used for demonstrating patterns
    internal interface IMyService
    {
        string GetValue(int id);
        Task<bool> SaveAsync(object user);
        int ExtraMethod(string name);
    }

    internal static void RunExamples()
    {
        // 1) Basic usage and chaining
        var basic = FluentSubstitute.For<IMyService>()
            .Configure(s => s.GetValue(Arg.Any<int>()).Returns("value"))
            .Configure(s => s.SaveAsync(Arg.Any<object>()).Returns(Task.FromResult(true)));

        var basicInstance = basic.Instance; // explicit access
        _ = basicInstance.GetValue(1); // retrieve value

        // 2) Conditional configuration - only configure ExtraMethod when needed
        var shouldConfigureExtra = false;
        var conditional = FluentSubstitute.For<IMyService>()
            .Configure(s => s.GetValue(1).Returns("one"))
            .ConfigureIf(shouldConfigureExtra, s => s.ExtraMethod(Arg.Any<string>()).Returns(123));

        var conditionalInstance = conditional.Instance;
        _ = conditionalInstance.ExtraMethod("x"); // returns default (0)

        // 3) Defaults + overrides - configure common defaults then override in tests
        var withDefaults = FluentSubstitute.For<IMyService>()
            .WithDefaults(s => s.SaveAsync(Arg.Any<object>()).Returns(Task.FromResult(false)))
            .Configure(s => s.SaveAsync(
                Arg.Is<object>(u => u != null && u.ToString()!.EndsWith("@test"))).Returns(Task.FromResult(true))
            );

        var defaultsInstance = withDefaults.Instance;
        _ = defaultsInstance; // keep analyzer happy about unused local

        // 4) Partial substitute for concrete class
        var partial = FluentSubstitute.ForParts<ExampleClass>("ctorArg")
            .ConfigureIf(true, p => p.VirtualMethod(Arg.Any<int>()).Returns(555));

        var pInst = partial.Instance;
        _ = pInst.VirtualMethod(5);
    }

    internal class ExampleClass
    {
        private readonly string _ctorArg;
        public ExampleClass(string ctorArg) => _ctorArg = ctorArg;
        public virtual int VirtualMethod(int x) => x + _ctorArg.Length;
        public int NonVirtualMethod(int x) => x * 2;
    }
}

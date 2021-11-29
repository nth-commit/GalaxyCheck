using System.Threading.Tasks;
using Xunit;
using Verifier = GalaxyCheck.Xunit.CodeAnalysis.Tests.Analyzers.Verifier<GalaxyCheck.Xunit.CodeAnalysis.Analyzers.MemberGenShouldReferenceValidMember>;

namespace GalaxyCheck.Xunit.CodeAnalysis.Tests.CodeAnalysis
{
    public class MemberGenShouldReferenceValidMemberTests
    {
        [Theory]
        [InlineData("IGen<int>", "int")]
        //[InlineData("IGen<string>", "object", Skip = "Current limitation")]
        public async Task NotError(string memberType, string parameterType)
        {
            var source = @"
using GalaxyCheck;

public class TestClass
{
    public static " + memberType + @" Gen => null!;

    [Property]
    public void TestMethod([MemberGen(""Gen"")] " + parameterType + @" x)
    {
    }
}";

            await Verifier.Verify(new[] { source });
        }

        [Fact]
        public async Task FindsError_MustReferenceExistingMember()
        {
            var source = @"
using GalaxyCheck;

public class TestClass
{
    [Property]
    public void TestMethod([MemberGen(""Gen"")] int x)
    {
    }
}";

            var expectedDiagnostic = Verifier
                .Diagnostic("GalaxyCheckXunit1000")
                .WithSpan(7, 29, 7, 45)
                .WithArguments("Gen", "TestClass");

            await Verifier.Verify(new[] { source }, expectedDiagnostic);
        }

        [Fact]
        public async Task FindsError_MemberMustBePropertyOrField()
        {
            var source = @"
using GalaxyCheck;

public class TestClass
{
    private static void AnotherMethod()
    {
    }

    [Property]
    public void TestMethod([MemberGen(""AnotherMethod"")] int x)
    {
    }
}";

            var expectedDiagnostic = Verifier
                .Diagnostic("GalaxyCheckXunit1001")
                .WithSpan(11, 29, 11, 55);

            await Verifier.Verify(new[] { source }, expectedDiagnostic);
        }

        [Theory]
        [InlineData("object")]
        [InlineData("GalaxyCheck.IGen<string>")]
        public async Task FindsError_MemberMustBeOfTypeIGen(string memberType)
        {
            var source = @"
using GalaxyCheck;

public class TestClass
{
    private static " + memberType + @" Gen => null!;

    [Property]
    public void TestMethod([MemberGen(""Gen"")] int x)
    {
    }
}";

            var expectedDiagnostic = Verifier
                .Diagnostic("GalaxyCheckXunit1002")
                .WithSpan(9, 29, 9, 45)
                .WithArguments("GalaxyCheck.IGen<int>", memberType);

            await Verifier.Verify(new[] { source }, expectedDiagnostic);
        }
    }
}

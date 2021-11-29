using Microsoft.CodeAnalysis.Text;
using System.Threading.Tasks;
using Xunit;
using Verifier = GalaxyCheck.Xunit.CodeAnalysis.Tests.CodeRefactoringProviders.Verifier<GalaxyCheck.Xunit.CodeAnalysis.CodeRefactoringProviders.ConfigureGenWithMember>;

namespace GalaxyCheck.Xunit.CodeAnalysis.Tests.CodeRefactoringProviders
{
    /// <summary>
    /// - Handle Lists with explicit generators, and other higher-order generators (nullable etc.)
    /// - Provide the refactor anywhere inside the method declaration node, having it provide an option for each argument
    /// </summary>
    public class ConfigureGenWithMemberTests
    {
        [Theory]
        [InlineData("byte", "Gen.Byte()")]
        [InlineData("short", "Gen.Int16()")]
        [InlineData("int", "Gen.Int32()")]
        [InlineData("long", "Gen.Int64()")]
        [InlineData("string", "Gen.String()")]
        [InlineData("char", "Gen.Char()")]
        [InlineData("bool", "Gen.Boolean()")]
        [InlineData("System.Guid", "Gen.Guid()")]
        [InlineData("System.DateTime", "Gen.DateTime()")]
        [InlineData("TestClass", "Gen.Create<TestClass>()")] // A random type, not generatable by default, which is in scope
        public async Task ItRefactorsVariousTypes(string type, string genExpression)
        {
            var code = @"
using GalaxyCheck;

public class TestClass
{
    [Property]
    public void TestMethod(" + type + @" x)
    {
    }
}
";
            var codeProvidingRefactorPosition = new LinePosition(7, 28);

            var expectedRefactoredCode = @"
using GalaxyCheck;

public class TestClass
{
    private static IGen<" + type + @"> TestMethod_x => " + genExpression + @";

    [Property]
    public void TestMethod([MemberGen(nameof(TestMethod_x))] " + type + @" x)
    {
    }
}
";

            var expectedRefactoringTitle = $"Configure generation of parameter with MemberGenAttribute";

            await Verifier.Verify(code, codeProvidingRefactorPosition, expectedRefactoredCode, expectedRefactoringTitle);
        }

        [Theory]
        [InlineData(7, 28)]
        [InlineData(7, 32)]
        public async Task ItRefactorsAtVariousPositions(int line, int character)
        {
            var code = @"
using GalaxyCheck;

public class TestClass
{
    [Property]
    public void TestMethod(int x)
    {
    }
}
";
            var codeProvidingRefactorPosition = new LinePosition(line, character);

            var expectedRefactoredCode = @"
using GalaxyCheck;

public class TestClass
{
    private static IGen<int> TestMethod_x => Gen.Int32();

    [Property]
    public void TestMethod([MemberGen(nameof(TestMethod_x))] int x)
    {
    }
}
";

            var expectedRefactoringTitle = $"Configure generation of parameter with MemberGenAttribute";

            await Verifier.Verify(code, codeProvidingRefactorPosition, expectedRefactoredCode, expectedRefactoringTitle);
        }

        [Theory]
        [InlineData(7, 27)]
        [InlineData(7, 33)]
        public async Task ItDoesNotRefactorAtVariousOtherPositions(int line, int character)
        {
            var code = @"
using GalaxyCheck;

public class TestClass
{
    [Property]
    public void TestMethod(int x)
    {
    }
}
";
            var position = new LinePosition(line, character);

            await Verifier.VerifyNotRefactored(code, position);
        }

        [Fact]
        public async Task ItDoesNotRefactorIfTheMemberIsAlreadyDecoratedWithAMemberGenAttribute()
        {
            var code = @"
using GalaxyCheck;

public class TestClass
{
    [Property]
    public void TestMethod([MemberGenAttribute(""Gen"")] int x)
    {
    }
}
";
            var codeProvidingRefactorPosition = new LinePosition(7, 28);

            await Verifier.VerifyNotRefactored(code, codeProvidingRefactorPosition);
        }
    }
}

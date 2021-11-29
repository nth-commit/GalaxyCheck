using Microsoft.CodeAnalysis.Text;
using System.Threading.Tasks;
using Xunit;
using Verifier = GalaxyCheck.Xunit.CodeAnalysis.Tests.CodeRefactoringProviders.Verifier<GalaxyCheck.Xunit.CodeAnalysis.CodeRefactoringProviders.ConfigureGenWithMember>;

namespace GalaxyCheck.Xunit.CodeAnalysis.Tests.CodeRefactoringProviders
{
    /// <summary>
    /// - Variables:
    ///     - Line position (anywhere within "int x")
    ///     - Parameter name/method name, property becomes "[MethodName]_[ParameterName]"
    /// - Don't add member gen attribute if it already exists
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
        public async Task ItInsertsAndReferencesTheMember(string type, string genExpression)
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
    }
}

using FluentAssertions;
using GalaxyCheck;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Tests.V2.PropertyTests.ReflectedPropertyTests
{
    public class AboutValidation
    {
        public class Sync
        {
            private record MyType();

            [Fact]
            public void AMethodReturningAnUnsupportedTypeThrowsTheGenericUnsupportedExceptionMessage()
            {
                Func<MyType> f = () => new MyType();

                Action test = () => Property.Reflect(f.Method, null);

                test.Should()
                    .Throw<Exception>()
                    .WithMessage($"Return type is not supported by Property.Reflect. Please use one of: GalaxyCheck.IGen`1[GalaxyCheck.Test], GalaxyCheck.Property, System.Boolean, System.Void. Return type was: {typeof(MyType)}.");
            }

            private Task TaskMethod() => null!;
            private Task<bool> TaskBoolMethod() => null!;
            private AsyncProperty AsyncPropertyMethod() => null!;
            private IGen<AsyncTest> IGenAsyncTestMethod() => null!;

            [Theory]
            [InlineData(nameof(TaskMethod))]
            [InlineData(nameof(TaskBoolMethod))]
            [InlineData(nameof(AsyncPropertyMethod))]
            [InlineData(nameof(IGenAsyncTestMethod))]
            public void AMethodReturningAnUnsupportedTypeSupportedByAsyncThrowsTheMessageWithHint(string methodName)
            {
                var method = GetMethod(methodName);

                Action test = () => Property.Reflect(method, null);

                test.Should()
                    .Throw<Exception>()
                    .WithMessage($"Return type is not supported by Property.Reflect. Did you mean to use Property.ReflectAsync? Otherwise, please use one of: GalaxyCheck.IGen`1[GalaxyCheck.Test], GalaxyCheck.Property, System.Boolean, System.Void. Return type was: {method.ReturnType}.");
            }

            private static MethodInfo GetMethod(string name)
            {
                var methodInfo = typeof(Sync).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);

                if (methodInfo == null)
                {
                    throw new Exception("Unable to locate method");
                }

                return methodInfo;
            }
        }

        public class Async
        {
            private record MyType();

            [Fact]
            public void AMethodReturningAnUnsupportedTypeThrowsTheGenericUnsupportedExceptionMessage()
            {
                Func<MyType> f = () => new MyType();

                Action test = () => Property.ReflectAsync(f.Method, null);

                test.Should()
                    .Throw<Exception>()
                    .WithMessage($"Return type is not supported by Property.ReflectAsync. Please use one of: GalaxyCheck.AsyncProperty, GalaxyCheck.IGen`1[GalaxyCheck.AsyncTest], GalaxyCheck.IGen`1[GalaxyCheck.Test], GalaxyCheck.Property, System.Boolean, System.Threading.Tasks.Task, System.Threading.Tasks.Task`1[System.Boolean], System.Void. Return type was: {typeof(MyType)}.");
            }
        }
    }
}

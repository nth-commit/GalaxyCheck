using FluentAssertions;
using System;
using System.Reflection;
using Xunit;

using GalaxyCheck;
using System.Threading.Tasks;

namespace Tests.V2.PropertyTests.ReflectedPropertyTests
{
    public class AboutMethodInfoOutput
    {
        public class Sync
        {
            private void AnInfallibleVoidPropertyFunction() { }

            [Fact]
            public void AVoidMethodInfoCanBeUnfalsifiable()
            {
                var property = Property.Reflect(GetMethod(nameof(AnInfallibleVoidPropertyFunction)), this);

                var checkResult = property.Check();

                checkResult.Falsified.Should().BeFalse();
            }

            private void AFallibleVoidPropertyFunction(int x)
            {
                Assert.True(x < 100);
            }

            [Fact]
            public void AVoidMethodInfoCanBeFalsified()
            {
                var property = Property.Reflect(GetMethod(nameof(AFallibleVoidPropertyFunction)), this);

                var checkResult = property.Check();

                checkResult.Falsified.Should().BeTrue();
            }

            private bool AnInfallibleBooleanPropertyFunction() => true;

            [Fact]
            public void ABooleanMethodInfoCanBeUnfalsifiable()
            {
                var property = Property.Reflect(GetMethod(nameof(AnInfallibleBooleanPropertyFunction)), this);

                var checkResult = property.Check();

                checkResult.Falsified.Should().BeFalse();
            }

            private bool AFallibleBooleanPropertyFunction(int x) => x < 100;

            [Fact]
            public void ABooleanMethodInfoCanBeFalsified()
            {
                var property = Property.Reflect(GetMethod(nameof(AFallibleBooleanPropertyFunction)), this);

                var checkResult = property.Check();

                checkResult.Falsified.Should().BeTrue();
            }

            private Property AnInfalliblePropertyFunction() => Gen.Constant<object?>(null).ForAll(_ => true);
            private Property AnInfalliblePropertyFunctionWithVaryingTypes() => Gen.Constant(true).ForAll(_ => true);

            [Theory]
            [InlineData(nameof(AnInfalliblePropertyFunction))]
            [InlineData(nameof(AnInfalliblePropertyFunctionWithVaryingTypes))]
            public void APropertyMethodInfoCanBeUnfalsifiable(string methodName)
            {
                var property = Property.Reflect(GetMethod(methodName), this);

                var checkResult = property.Check();

                checkResult.Falsified.Should().BeFalse();
            }

            private Property AFalliblePropertyFunction() => Gen.Int32().ForAll(x => x < 100);

            [Fact]
            public void APropertyMethodInfoCanBeFalsified()
            {
                var property = Property.Reflect(GetMethod(nameof(AFalliblePropertyFunction)), this);

                var checkResult = property.Check();

                checkResult.Falsified.Should().BeTrue();
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
            private void AnInfallibleVoidPropertyFunction() { }

            [Fact]
            public void AVoidMethodInfoCanBeUnfalsifiable()
            {
                var property = Property.ReflectAsync(GetMethod(nameof(AnInfallibleVoidPropertyFunction)), this);

                Action test = () =>
                {
                    property.CheckAsync();
                };

                test.Should().Throw<NotImplementedException>();

                //var checkResult = property.CheckAsync();

                //checkResult.Falsified.Should().BeFalse();
            }

            private void AFallibleVoidPropertyFunction(int x)
            {
                Assert.True(x < 100);
            }

            [Fact]
            public void AVoidMethodInfoCanBeFalsified()
            {
                var property = Property.ReflectAsync(GetMethod(nameof(AFallibleVoidPropertyFunction)), this);

                Action test = () =>
                {
                    property.CheckAsync();
                };

                test.Should().Throw<NotImplementedException>();

                //var checkResult = property.CheckAsync();

                //checkResult.Falsified.Should().BeTrue();
            }

            private bool AnInfallibleBooleanPropertyFunction() => true;

            [Fact]
            public void ABooleanMethodInfoCanBeUnfalsifiable()
            {
                var property = Property.ReflectAsync(GetMethod(nameof(AnInfallibleBooleanPropertyFunction)), this);

                Action test = () =>
                {
                    property.CheckAsync();
                };

                test.Should().Throw<NotImplementedException>();

                //var checkResult = property.CheckAsync();

                //checkResult.Falsified.Should().BeFalse();
            }

            private bool AFallibleBooleanPropertyFunction(int x) => x < 100;

            [Fact]
            public void ABooleanMethodInfoCanBeFalsified()
            {
                var property = Property.ReflectAsync(GetMethod(nameof(AFallibleBooleanPropertyFunction)), this);

                Action test = () =>
                {
                    property.CheckAsync();
                };

                test.Should().Throw<NotImplementedException>();

                //var checkResult = property.CheckAsync();

                //checkResult.Falsified.Should().BeTrue();
            }

            private Property AnInfalliblePropertyFunction() => Gen.Constant<object?>(null).ForAll(_ => true);
            private Property AnInfalliblePropertyFunctionWithVaryingTypes() => Gen.Constant(true).ForAll(_ => true);

            [Theory]
            [InlineData(nameof(AnInfalliblePropertyFunction))]
            [InlineData(nameof(AnInfalliblePropertyFunctionWithVaryingTypes))]
            public void APropertyMethodInfoCanBeUnfalsifiable(string methodName)
            {
                var property = Property.ReflectAsync(GetMethod(methodName), this);

                Action test = () =>
                {
                    property.CheckAsync();
                };

                test.Should().Throw<NotImplementedException>();

                //var checkResult = property.CheckAsync();

                //checkResult.Falsified.Should().BeFalse();
            }

            private Property AFalliblePropertyFunction() => Gen.Int32().ForAll(x => x < 100);

            [Fact]
            public void APropertyMethodInfoCanBeFalsified()
            {
                var property = Property.ReflectAsync(GetMethod(nameof(AFalliblePropertyFunction)), this);

                Action test = () =>
                {
                    property.CheckAsync();
                };

                test.Should().Throw<NotImplementedException>();

                //var checkResult = property.CheckAsync();

                //checkResult.Falsified.Should().BeTrue();
            }

            private Task AnInfallibleTaskPropertyFunction() => Task.CompletedTask;

            [Fact]
            public void ATaskMethodInfoCanBeUnfalsifiable()
            {
                var property = Property.ReflectAsync(GetMethod(nameof(AnInfallibleTaskPropertyFunction)), this);

                Action test = () =>
                {
                    property.CheckAsync();
                };

                test.Should().Throw<NotImplementedException>();

                //var checkResult = property.CheckAsync();

                //checkResult.Falsified.Should().BeFalse();
            }

            private async Task AFallibleTaskPropertyFunction(int x)
            {
                await Task.Delay(1);
                Assert.True(x < 100);
            }

            [Fact]
            public void ATaskMethodInfoCanBeFalsified()
            {
                var property = Property.ReflectAsync(GetMethod(nameof(AFallibleTaskPropertyFunction)), this);

                Action test = () =>
                {
                    property.CheckAsync();
                };

                test.Should().Throw<NotImplementedException>();

                //var checkResult = property.CheckAsync();

                //checkResult.Falsified.Should().BeTrue();
            }

            private async Task<bool> AnInfallibleTaskBooleanPropertyFunction()
            {
                await Task.Delay(1);
                return true;
            }

            [Fact]
            public void ATaskBooleanMethodInfoCanBeUnfalsifiable()
            {
                var property = Property.ReflectAsync(GetMethod(nameof(AnInfallibleTaskBooleanPropertyFunction)), this);

                Action test = () =>
                {
                    property.CheckAsync();
                };

                test.Should().Throw<NotImplementedException>();

                //var checkResult = property.CheckAsync();

                //checkResult.Falsified.Should().BeFalse();
            }

            private async Task<bool> AFallibleTaskBooleanPropertyFunction(int x)
            {
                await Task.Delay(1);
                return x < 100;
            }

            [Fact]
            public void ATaskBooleanMethodInfoCanBeFalsified()
            {
                var property = Property.ReflectAsync(GetMethod(nameof(AFallibleTaskBooleanPropertyFunction)), this);

                Action test = () =>
                {
                    property.CheckAsync();
                };

                test.Should().Throw<NotImplementedException>();

                //var checkResult = property.CheckAsync();

                //checkResult.Falsified.Should().BeTrue();
            }

            private AsyncProperty AnInfallibleAsyncPropertyFunction() => Gen.Constant<object?>(null).ForAllAsync(async _ =>
            {
                await Task.Delay(1);
                return true;
            });
            private AsyncProperty AnInfallibleAsyncPropertyFunctionWithVaryingTypes() => Gen.Constant(true).ForAllAsync(async _ =>
            {
                await Task.Delay(1);
                return true;
            });

            [Theory]
            [InlineData(nameof(AnInfallibleAsyncPropertyFunction))]
            [InlineData(nameof(AnInfallibleAsyncPropertyFunctionWithVaryingTypes))]
            public void AnAsyncPropertyMethodInfoCanBeUnfalsifiable(string methodName)
            {
                var property = Property.ReflectAsync(GetMethod(methodName), this);

                Action test = () =>
                {
                    property.CheckAsync();
                };

                test.Should().Throw<NotImplementedException>();

                //var checkResult = property.CheckAsync();

                //checkResult.Falsified.Should().BeFalse();
            }

            private AsyncProperty AFallibleAsyncPropertyFunction() => Gen.Int32().ForAllAsync(async x =>
            {
                await Task.Delay(1);
                return x < 100;
            });

            [Fact]
            public void AnAsyncPropertyMethodInfoCanBeFalsified()
            {
                var property = Property.ReflectAsync(GetMethod(nameof(AFallibleAsyncPropertyFunction)), this);

                Action test = () =>
                {
                    property.CheckAsync();
                };

                test.Should().Throw<NotImplementedException>();

                //var checkResult = property.CheckAsync();

                //checkResult.Falsified.Should().BeTrue();
            }

            private static MethodInfo GetMethod(string name)
            {
                var methodInfo = typeof(Async).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);

                if (methodInfo == null)
                {
                    throw new Exception("Unable to locate method");
                }

                return methodInfo;
            }
        }
    }
}

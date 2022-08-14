using GalaxyCheck.Configuration;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace GalaxyCheck.Internal
{
    internal static class GenSnapshotRunner
    {
        public static Task<(Exception?, decimal)> Run(
            GenSnapshotInitializationResult initialization,
            Type testClass,
            MethodInfo testMethod,
            int seedIndex) => RunTestFunction(() => RunSnapshot(
                initialization.TestFileName,
                testClass.Name,
                testMethod.Name,
                (parameters) => InvokeTestMethod(initialization.TestClassInstance, testMethod, parameters),
                initialization.AssertSnapshotMatches,
                initialization.ParametersGen,
                initialization.Seeds[seedIndex],
                initialization.Size));

        private static async Task<(Exception?, decimal)> RunTestFunction(Func<Task> testFunction)
        {
            var startTime = DateTime.UtcNow;

            Exception? exception = null;
            decimal executionTime;
            try
            {
                await testFunction();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                executionTime = (decimal)(DateTime.UtcNow - startTime).TotalSeconds;
            }

            return (exception, executionTime);
        }

        private static async Task<object?> InvokeTestMethod(
            object testClassInstance,
            MethodInfo testMethodInfo,
            object[] parameters)
        {
            var result = testMethodInfo.Invoke(testClassInstance, parameters);

            return await DynamicTaskUnwrap(result);
        }

        private static async Task<object?> DynamicTaskUnwrap(object? result)
        {
            if (result == null) return result;

            var resultType = result.GetType();
            if (resultType.IsGenericType == false) return result;

            var genericArgument = resultType.GetGenericArguments().First();
                 
            var genericTask = typeof(Task<>).MakeGenericType(genericArgument);
            if (resultType.IsAssignableTo(genericTask))
            {
                await(Task)result;
                return genericTask.GetProperty(nameof(Task<object>.Result))!.GetValue(result);
            }

            var genericValueTask = typeof(ValueTask<>).MakeGenericType(genericArgument);
            if (resultType.IsAssignableTo(genericValueTask))
            {
                await((Task)genericValueTask.GetMethod(nameof(ValueTask<object>.AsTask))!.Invoke(result, Array.Empty<object>())!);
                return genericValueTask.GetProperty(nameof(ValueTask<object>.Result))!.GetValue(result);
            }

            return result;
        }

        private static async Task RunSnapshot(
            string testFileName,
            string testClassName,
            string testMethodName,
            Func<object[], Task<object?>> invokeTestMethod,
            Func<ISnapshot, Task> assertSnapshotMatches,
            IGen<object[]> parametersGen,
            int seed,
            int size)
        {
            var input = parametersGen.Advanced.SetRngWaypoint().SampleOne(seed: seed, size: size);
            var output = await invokeTestMethod(input);
            var snapshot = new Snapshot(input, output, testFileName, testClassName, testMethodName, seed);
            await assertSnapshotMatches(snapshot);
        }

        private record Snapshot(
            object?[] Input,
            object? Output,
            string TestFileName,
            string TestClassName,
            string TestMethodName,
            int Seed) : ISnapshot;
    }
}

using GalaxyCheck.Gens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GalaxyCheck.Internal
{
    internal static class ReflectionHelpers
    {
        public static IGenFactory? TryResolveGenFactory(Type testClassType, MethodInfo testMethodInfo)
        {
            var genFactoryAttribute =
                testMethodInfo.GetCustomAttributes<GenFactoryAttribute>(inherit: true).FirstOrDefault() ??
                testClassType.GetCustomAttributes<GenFactoryAttribute>(inherit: true).FirstOrDefault();

            return genFactoryAttribute?.Value;
        }

        public static IReadOnlyDictionary<int, IGen> ResolveCustomGens(object testClassInstance, MethodInfo methodInfo)
        {
            return methodInfo
                .GetParameters()
                .Select((parameter, i) =>
                {
                    var memberGenAttribute = parameter.GetCustomAttribute<MemberGenAttribute>();
                    return memberGenAttribute == null
                        ? null :
                        new { gen = ResolveMemberGen(testClassInstance, memberGenAttribute.MemberName), parameterIndex = i };
                })
                .Where(x => x != null)
                .ToDictionary(x => x!.parameterIndex, x => x!.gen);
        }

        private static IGen ResolveMemberGen(object target, string memberName)
        {
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

            var property = target.GetType().GetProperty(memberName, bindingFlags);
            if (property != null)
            {
                return CastToGen(property.GetValue(target)!, memberName);
            }

            var field = target.GetType().GetField(memberName, bindingFlags);
            if (field != null)
            {
                return CastToGen(field.GetValue(target)!, memberName);
            }

            return Gen.Constant<object?>(null);

            static IGen CastToGen(object value, string memberName)
            {
                var gen = value as IGen;

                if (gen == null)
                {
                    throw new Exception($"Expected member '{memberName}' to be an instance of '{typeof(IGen).FullName}', but it had a value of type '{value.GetType().FullName}'");
                }

                return gen;
            }
        }
    }
}

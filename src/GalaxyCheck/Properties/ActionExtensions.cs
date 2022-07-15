using System;
using System.Threading.Tasks;

namespace GalaxyCheck.Properties
{
    internal static class ActionExtensions
    {
        public static Func<bool> AsTrueFunc(this Action action) => () =>
        {
            action();
            return true;
        };

        public static Func<T, bool> AsTrueFunc<T>(this Action<T> action) => (arg) =>
        {
            action(arg);
            return true;
        };

        public static Func<T0, T1, bool> AsTrueFunc<T0, T1>(this Action<T0, T1> action) => (arg0, arg1) =>
        {
            action(arg0, arg1);
            return true;
        };

        public static Func<T0, T1, T2, bool> AsTrueFunc<T0, T1, T2>(this Action<T0, T1, T2> action) => (arg0, arg1, arg2) =>
        {
            action(arg0, arg1, arg2);
            return true;
        };

        public static Func<T0, T1, T2, T3, bool> AsTrueFunc<T0, T1, T2, T3>(this Action<T0, T1, T2, T3> action) => (arg0, arg1, arg2, arg3) =>
        {
            action(arg0, arg1, arg2, arg3);
            return true;
        };

        public static Func<Task<bool>> AsTrueFunc(this Func<Task> action) => async () =>
        {
            await action();
            return true;
        };

        public static Func<T, Task<bool>> AsTrueFunc<T>(this Func<T, Task> action) => async (arg) =>
        {
            await action(arg);
            return true;
        };

        public static Func<T0, T1, Task<bool>> AsTrueFunc<T0, T1>(this Func<T0, T1, Task> action) => async (arg0, arg1) =>
        {
            await action(arg0, arg1);
            return true;
        };

        public static Func<T0, T1, T2, Task<bool>> AsTrueFunc<T0, T1, T2>(this Func<T0, T1, T2, Task> action) => async (arg0, arg1, arg2) =>
        {
            await action(arg0, arg1, arg2);
            return true;
        };

        public static Func<T0, T1, T2, T3, Task<bool>> AsTrueFunc<T0, T1, T2, T3>(this Func<T0, T1, T2, T3, Task> action) => async (arg0, arg1, arg2, arg3) =>
        {
            await action(arg0, arg1, arg2, arg3);
            return true;
        };
    }
}

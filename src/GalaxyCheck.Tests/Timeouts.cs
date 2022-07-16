using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Tests.V2
{
    public static class Timeouts
    {
        public static void RunWithTimeout(Action action, TimeSpan timeout)
        {
            var task = Task.Run(action);
            try
            {
                var success = task.Wait(timeout);
                if (!success)
                {
                    throw new TimeoutException();
                }
            }
            catch (AggregateException ex) when (ex.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            }
        }

        public static void RunWithTimeoutAsync(Func<Task> action, TimeSpan timeout)
        {
            var task = Task.Run(action);
            try
            {
                var success = task.Wait(timeout);
                if (!success)
                {
                    throw new TimeoutException();
                }
            }
            catch (AggregateException ex) when (ex.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            }
        }
    }
}

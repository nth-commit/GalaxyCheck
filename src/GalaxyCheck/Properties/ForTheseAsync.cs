using GalaxyCheck.Properties;
using System;
using System.Threading.Tasks;

namespace GalaxyCheck
{
    public partial class Property
    {
        public static TestAsync<object> ForTheseAsync(Func<Task<bool>> func) => TestFactory.CreateAsync<object>(
            null!,
            () => func(),
            null);

        public static TestAsync<object> ForTheseAsync(Func<Task> func) => ForTheseAsync(func.AsTrueFunc());
    }
}

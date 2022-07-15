using GalaxyCheck.Properties;
using System;
using System.Threading.Tasks;

namespace GalaxyCheck
{
    public partial class Property
    {
        public static AsyncTest<object> ForTheseAsync(Func<Task<bool>> func) => TestFactory.Create<object>(
            null!,
            () => func(),
            null);

        public static AsyncTest<object> ForTheseAsync(Func<Task> func) => ForTheseAsync(func.AsTrueFunc());
    }
}

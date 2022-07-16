using GalaxyCheck.Properties;
using System;
using System.Threading.Tasks;

namespace GalaxyCheck
{
    public partial class Property
    {
        public static AsyncTest ForTheseAsync(Func<Task<bool>> func) => TestFactory.Create(
            null!,
            () => new ValueTask<bool>(func()),
            null);

        public static AsyncTest ForTheseAsync(Func<Task> func) => ForTheseAsync(func.AsTrueFunc());
    }
}

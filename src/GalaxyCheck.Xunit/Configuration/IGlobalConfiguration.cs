using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalaxyCheck.Configuration
{
    public interface IGlobalConfiguration
    {
        int DefaultIterations { get; set; }

        int DefaultShrinkLimit { get; set; }
    }
}

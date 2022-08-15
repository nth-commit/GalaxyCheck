using GalaxyCheck;
using GalaxyCheck.Configuration;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace GalaxyCheck.Internal
{
    internal class GlobalConfiguration : IGlobalConfiguration
    {
        public IGlobalPropertyConfiguration Properties { get; } = new GlobalPropertyConfiguration();

        public IGlobalGenSnapshotConfiguration GenSnapshots { get; } = new GlobalGenSnapshotConfiguration();

        private static ConcurrentDictionary<Assembly, GlobalConfiguration> _instancesByAssembly = new();

        public static GlobalConfiguration GetInstance(Assembly assembly) => _instancesByAssembly.GetOrAdd(assembly, CreateConfigurationForAssembly);

        private static GlobalConfiguration CreateConfigurationForAssembly(Assembly assembly)
        {
            var instance = new GlobalConfiguration();

            var configureGlobalTypes =
                from type in assembly.GetTypes()
                where type.GetInterfaces().Contains(typeof(IConfigureGlobal))
                select type;

            foreach (var configureGlobalType in configureGlobalTypes)
            {
                IConfigureGlobal configureGlobal;
                try
                {
                    configureGlobal = (IConfigureGlobal)Activator.CreateInstance(configureGlobalType)!;
                }
                catch (Exception ex)
                {
                    throw new ActivateConfigureGlobalException(configureGlobalType, ex);
                }

                configureGlobal.Configure(instance);
            }

            return instance;
        }
    }
}

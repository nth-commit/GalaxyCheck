using GalaxyCheck;
using GalaxyCheck.Configuration;
using System;
using System.Linq;

namespace GalaxyCheck.Internal
{
    internal class GlobalConfiguration : IGlobalConfiguration
    {
        public int DefaultIterations { get; set; } = 100;

        public int DefaultShrinkLimit { get; set; } = 100;

        private static object _instanceLock = new();
        private static GlobalConfiguration? _instance = null;

        public static GlobalConfiguration Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_instanceLock)
                    {
                        if (_instance == null)
                        {
                            _instance = CreateInstance();
                        }
                    }
                }

                return _instance;
            }
        }

        private static GlobalConfiguration CreateInstance()
        {
            var instance = new GlobalConfiguration();

            var configureGlobalTypes =
                from assembly in AppDomain.CurrentDomain.GetAssemblies()
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

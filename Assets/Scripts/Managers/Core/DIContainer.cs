using System;
using System.Collections.Generic;

namespace MultiplayFishing.Core
{
    public static class DIContainer
    {
        private static readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

        public static void Register<T>(object implementation)
        {
            var type = typeof(T);
            if (services.ContainsKey(type))
            {
                services[type] = implementation;
            }
            else
            {
                services.Add(type, implementation);
            }
        }

        public static T Resolve<T>()
        {
            var type = typeof(T);
            if (services.TryGetValue(type, out var implementation))
            {
                return (T)implementation;
            }
            throw new Exception($"Service {type.Name} not registered in DIContainer.");
        }

        public static bool TryResolve<T>(out T service)
        {
            var type = typeof(T);
            if (services.TryGetValue(type, out var implementation))
            {
                service = (T)implementation;
                return true;
            }

            service = default;
            return false;
        }
    }
}

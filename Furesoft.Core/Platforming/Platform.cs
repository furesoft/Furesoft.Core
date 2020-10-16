using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Furesoft.Core.Platforming
{
    public static class Platform
    {
        public static T New<T>(params object[] args)
        {
            var currentPlatform = GetCurrentPlatform();
            var implementation = GetImplementationOf<T>(currentPlatform);

            return Injector.Get<T>(implementation);
        }

        private static Type GetImplementationOf<T>(OSName currentPlatform)
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(_ => _.GetTypes());

            foreach (var t in types)
            {
                if (t.IsInterface || t.IsAbstract) continue;
                else
                {
                    if (typeof(T).IsAssignableFrom(t) || t.IsInstanceOfType(typeof(T)))
                    {
                        var attr = t.GetCustomAttribute<PlattformImplementationAttribute>();
                        if (attr != null)
                        {
                            if (attr.Platform == currentPlatform)
                            {
                                return t;
                            }
                        }
                    }
                }
            }

            throw new PlatformNotSupportedException();
        }

        private static OSName GetCurrentPlatform()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return OSName.Windows;
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return OSName.Linux;
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return OSName.OSX;
            }

            return OSName.Windows;
        }
    }
}
using System;
using System.Collections.Generic;

namespace Furesoft.Core.Factory
{
    public static class ObjectFactory
    {
        private static readonly Dictionary<Type, IObjectFactory> factories = new Dictionary<Type, IObjectFactory>();

        public static T Create<T>(params object[] args)
        {
            var type = typeof(T);

            if (type.IsAbstract)
            {
                throw new InvalidOperationException($"Can't create instance of abstract class '{type}'");
            }

            var baseType = GetBaseType<T>();
            if (factories.ContainsKey(baseType))
            {
                return (T)factories[baseType].Create<T>(args);
            }

            throw new InvalidOperationException($"No Factory registered for Type '{type}'");
        }

        public static IObjectFactory GetFactoryOf<T>()
        {
            var baseType = GetBaseType<T>();
            if (factories.ContainsKey(baseType))
            {
                return factories[GetBaseType<T>()];
            }

            return null;
        }

        public static UFactory GetFactoryOf<T, UFactory>()
            where UFactory : IObjectFactory
        {
            return (UFactory)GetFactoryOf<T>();
        }

        public static bool IsRegisteredFor<TObject>()
        {
            return factories.ContainsKey(GetBaseType<TObject>());
        }

        public static void Register<TFactory, UResult>()
                    where TFactory : IObjectFactory
        {
            var type = typeof(UResult);
            if (!factories.ContainsKey(type))
            {
                var instance = Activator.CreateInstance<TFactory>();
                factories.Add(type, instance);
                return;
            }

            throw new InvalidOperationException("Factory is already registered");
        }

        private static Type GetBaseType<T>()
        {
            foreach (var t in factories.Keys)
            {
                if (t.IsAssignableFrom(typeof(T)))
                {
                    return t;
                }
            }

            return typeof(T);
        }
    }
}
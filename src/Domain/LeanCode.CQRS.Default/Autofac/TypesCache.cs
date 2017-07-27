using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace LeanCode.CQRS.Default.Autofac
{
    using GenericsProvider = Func<Type, Type, Type[]>;
    using CacheDictionary = ConcurrentDictionary<(Type ContextType, Type ObjType), (Type HandlerType, ConstructorInfo Constructor)>;

    sealed class TypesCache
    {
        private readonly CacheDictionary cache = new CacheDictionary();

        private readonly Func<(Type ContextType, Type ObjType), (Type HandlerType, ConstructorInfo Constructor)> make;

        public TypesCache(Type handlerBase, Type wrapperBase)
            : this(DefaultGetTypes, handlerBase, wrapperBase)
        { }

        public TypesCache(GenericsProvider provider, Type handlerBase, Type wrapperBase)
        {
            this.make = a =>
            {
                var types = provider(a.ContextType, a.ObjType);
                var handlerType = handlerBase.MakeGenericType(types);
                var wrapperType = wrapperBase.MakeGenericType(types);
                var ctor = wrapperType.GetConstructors()[0];
                return (handlerType, ctor);
            };
        }

        public (Type HandlerType, ConstructorInfo Constructor) Get(
            Type contextType, Type objType)
        {
            return cache.GetOrAdd((contextType, objType), make);
        }

        private static Type[] DefaultGetTypes(Type contextType, Type objType)
        {
            return new[] { contextType, objType };
        }
    }
}

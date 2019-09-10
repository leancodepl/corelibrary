using System;
using System.Reflection;

using CacheDictionary = System.Collections.Concurrent.ConcurrentDictionary<System.Type, (System.Type HandlerType, System.Reflection.ConstructorInfo Constructor)>;

namespace LeanCode.CQRS.Default.Autofac
{
    internal sealed class TypesCache
    {
        private readonly CacheDictionary cache = new CacheDictionary();

        private readonly Func<Type, (Type HandlerType, ConstructorInfo Constructor)> make;

        public TypesCache(Func<Type, Type[]> mkTypes, Type handlerBase, Type wrapperBase)
        {
            make = a =>
            {
                var types = mkTypes(a);
                var handlerType = handlerBase.MakeGenericType(types);
                var wrapperType = wrapperBase.MakeGenericType(types);
                var ctor = wrapperType.GetConstructors()[0];

                return (handlerType, ctor);
            };
        }

        public TypesCache(Func<Type, Type> handlerBaseMaker, Func<Type, Type> wrapperBaseMaker)
        {
            make = a =>
            {
                var handlerType = handlerBaseMaker(a);
                var wrapperType = wrapperBaseMaker(a);
                var ctor = wrapperType.GetConstructors()[0];

                return (handlerType, ctor);
            };
        }

        public (Type HandlerType, ConstructorInfo Constructor) Get(Type objType) =>
            cache.GetOrAdd(objType, make);
    }
}

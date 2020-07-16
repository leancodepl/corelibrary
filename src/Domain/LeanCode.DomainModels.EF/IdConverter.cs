using System;
using LeanCode.DomainModels.Model;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LeanCode.DomainModels.EF
{
    public class IdConverter<T> : ValueConverter<Id<T>, Guid>
        where T : class, IIdentifiable<Id<T>>
    {
        public static readonly IdConverter<T> Instance = new IdConverter<T>();

        private IdConverter()
            : base(
                d => d.Value,
                d => new Id<T>(d),
                mappingHints: null)
        { }
    }

    public class IIdConverter<T> : ValueConverter<IId<T>, int>
        where T : class, IIdentifiable<IId<T>>
    {
        public static readonly IIdConverter<T> Instance = new IIdConverter<T>();

        private IIdConverter()
            : base(
                d => d.Value,
                d => new IId<T>(d),
                mappingHints: null)
        { }
    }
}

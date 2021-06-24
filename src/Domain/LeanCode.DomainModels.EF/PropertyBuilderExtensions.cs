using LeanCode.DomainModels.Model;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeanCode.DomainModels.EF
{
    public static class PropertyBuilderExtensions
    {
        public static PropertyBuilder<Id<T>> IsTypedId<T>(this PropertyBuilder<Id<T>> builder)
            where T : class, IIdentifiable<Id<T>>
        {
            return builder.HasConversion(IdConverter<T>.Instance);
        }

        public static PropertyBuilder<Id<T>?> IsTypedId<T>(this PropertyBuilder<Id<T>?> builder)
            where T : class, IIdentifiable<Id<T>>
        {
            return builder.HasConversion(IdConverter<T>.Instance);
        }

        public static PropertyBuilder<IId<T>> IsTypedId<T>(this PropertyBuilder<IId<T>> builder)
            where T : class, IIdentifiable<IId<T>>
        {
            return builder.HasConversion(IIdConverter<T>.Instance);
        }

        public static PropertyBuilder<IId<T>?> IsTypedId<T>(this PropertyBuilder<IId<T>?> builder)
            where T : class, IIdentifiable<IId<T>>
        {
            return builder.HasConversion(IIdConverter<T>.Instance);
        }
    }
}

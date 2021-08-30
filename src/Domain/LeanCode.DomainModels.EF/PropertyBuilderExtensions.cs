using LeanCode.DomainModels.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeanCode.DomainModels.EF
{
    public static class PropertyBuilderExtensions
    {
        public static PropertyBuilder<Id<T>> IsTypedId<T>(this PropertyBuilder<Id<T>> builder)
            where T : class, IIdentifiable<Id<T>>
        {
            return builder.HasConversion(IdConverter<T>.Instance).ValueGeneratedNever();
        }

        public static PropertyBuilder<Id<T>?> IsTypedId<T>(this PropertyBuilder<Id<T>?> builder)
            where T : class, IIdentifiable<Id<T>>
        {
            return builder.HasConversion(IdConverter<T>.Instance).ValueGeneratedNever();
        }

        public static PropertyBuilder<IId<T>> IsTypedId<T>(this PropertyBuilder<IId<T>> builder)
            where T : class, IIdentifiable<IId<T>>
        {
            return builder.HasConversion(IIdConverter<T>.Instance).ValueGeneratedNever();
        }

        public static PropertyBuilder<IId<T>?> IsTypedId<T>(this PropertyBuilder<IId<T>?> builder)
            where T : class, IIdentifiable<IId<T>>
        {
            return builder.HasConversion(IIdConverter<T>.Instance).ValueGeneratedNever();
        }

        public static PropertyBuilder<LId<T>> IsTypedId<T>(this PropertyBuilder<LId<T>> builder)
            where T : class, IIdentifiable<LId<T>>
        {
            return builder.HasConversion(LIdConverter<T>.Instance).ValueGeneratedNever();
        }

        public static PropertyBuilder<LId<T>?> IsTypedId<T>(this PropertyBuilder<LId<T>?> builder)
            where T : class, IIdentifiable<LId<T>>
        {
            return builder.HasConversion(LIdConverter<T>.Instance).ValueGeneratedNever();
        }

        public static PropertyBuilder<SId<T>> IsTypedId<T>(this PropertyBuilder<SId<T>> builder)
            where T : class, IIdentifiable<SId<T>>
        {
            return builder
                .HasConversion(SIdConverter<T>.Instance)
                .HasMaxLength(SId<T>.RawLength)
                .IsFixedLength()
                .ValueGeneratedNever();
        }

        public static PropertyBuilder<SId<T>?> IsTypedId<T>(this PropertyBuilder<SId<T>?> builder)
            where T : class, IIdentifiable<SId<T>>
        {
            return builder
                .HasConversion(SIdConverter<T>.Instance)
                .HasMaxLength(SId<T>.RawLength)
                .IsFixedLength()
                .ValueGeneratedNever();
        }
    }
}

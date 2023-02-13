using LeanCode.DomainModels.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeanCode.DomainModels.EF;

public static class PropertiesConfigurationBuilderExtensions
{
    public static PropertiesConfigurationBuilder<SUlid<T>> AreTypedId<T>(this PropertiesConfigurationBuilder<SUlid<T>> builder)
        where T : class, IIdentifiable<SUlid<T>>
    {
        return builder
            .HaveConversion<SUlidConverter<T>>()
            .HaveMaxLength(SUlid<T>.RawLength)
            .AreFixedLength();
    }

    public static PropertiesConfigurationBuilder<SUlid<T>?> AreTypedId<T>(this PropertiesConfigurationBuilder<SUlid<T>?> builder)
        where T : class, IIdentifiable<SUlid<T>>
    {
        return builder
            .HaveConversion<SUlidConverter<T>>()
            .HaveMaxLength(SUlid<T>.RawLength)
            .AreFixedLength();
    }

    public static PropertiesConfigurationBuilder<Ulid<T>> AreTypedId<T>(this PropertiesConfigurationBuilder<Ulid<T>> builder)
        where T : class, IIdentifiable<Ulid<T>>
    {
        return builder
            .HaveConversion<UlidConverter<T>>()
            .HaveMaxLength(26)
            .AreFixedLength();
    }

    public static PropertiesConfigurationBuilder<Ulid<T>?> AreTypedId<T>(this PropertiesConfigurationBuilder<Ulid<T>?> builder)
        where T : class, IIdentifiable<Ulid<T>>
    {
        return builder
            .HaveConversion<UlidConverter<T>>()
            .HaveMaxLength(26)
            .AreFixedLength();
    }
}

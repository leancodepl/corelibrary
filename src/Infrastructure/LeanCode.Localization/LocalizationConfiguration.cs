using System;

namespace LeanCode.Localization;

public sealed class LocalizationConfiguration
{
    public Type ResourceSource { get; }

    public LocalizationConfiguration(Type resourceSource)
    {
        ResourceSource = resourceSource;
    }

    public static LocalizationConfiguration For<T>() =>
        new LocalizationConfiguration(typeof(T));
}

# Localization

Effectively handling localization is crucial in ensuring software compatibility across diverse linguistic and cultural contexts. The `LeanCode.Localization` package, integrated within the CoreLibrary, provides a solution for managing language-specific content. This integration simplifies the configuration and customization of localization, facilitating the localization of [emails] and [push notifications] with ease.

## Packages

| Package | Link | Application in section |
| --- | ----------- | ----------- |
| LeanCode.Localization | [![NuGet version (LeanCode.Localization)](https://img.shields.io/nuget/vpre/LeanCode.Localization.svg?style=flat-square)](https://www.nuget.org/packages/LeanCode.Localization/8.0.2260-preview/) | Localization |

## Configuration

To begin, let's introduce the `Strings.cs` marker class:

```csharp
public class Strings { }
```

Subsequently, we create the `Strings.resx` which serves as the default localization file, and should be placed in the same directory as the `Strings.cs` marker class:

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <data name="meeting-started" xml:space="preserve">
    <value>Meeting {0} has started.</value>
  </data>
</root>
```

For German-specific strings, we define the `Strings.de.resx` file:

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <data name="meeting-started" xml:space="preserve">
    <value>Besprechung {0} hat begonnen.</value>
  </data>
</root>
```

To register the localizer, simply follow the configuration outlined below:

```csharp
public override void ConfigureServices(IServiceCollection services)
{
    // . . .

    services.AddStringLocalizer(
        LocalizationConfiguration.For<Strings>());

    // . . .
}
```

Configuration above enables us to send localized and parameterized [push notifications], [emails] and extract language specific strings using `IStringLocalizer` interface:

```csharp
public class Example
{
    private readonly IStringLocalizer localizer;

    public Example(IStringLocalizer localizer)
    {
        this.localizer = localizer;
    }

    public void Method()
    {
        var key = "meeting-started";

        var defaultValue = localizer[CultureInfo.InvariantCulture, key];

        //  Meeting {0} has started.
        Console.WriteLine(defaultValue);

        var localizedValue = localizer[new CultureInfo("de-DE"), key];

        // Besprechung {0} hat begonnen.
        Console.WriteLine(localizedValue);

        var localizedParameterizedValue = localizer.Format(
            new CultureInfo("de-DE"),
            key,
            "Name der Besprechung");

        // Besprechung Name der Besprechung hat begonnen.
        Console.WriteLine(localizedParameterizedValue);
    }
}
```

[push notifications]: ../../external_integrations/push_notifications_fcm/index.md
[emails]: ../../external_integrations/emails_sendgrid/index.md

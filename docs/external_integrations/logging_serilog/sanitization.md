# Sanitization

In the context of logging, sanitization refers to the process of ensuring that sensitive or confidential information within log messages is appropriately handled to prevent exposure or leakage. LeanCode CoreLibrary Serilog integration provides a dedicated feature set for sanitization, offering mechanisms to safeguard sensitive data before logging it.

## Packages

| Package | Link | Application in section |
| --- | ----------- | ----------- |
| LeanCode.Logging | [![NuGet version (LeanCode.Logging)](https://img.shields.io/nuget/vpre/LeanCode.SendGrid.svg?style=flat-square)](https://www.nuget.org/packages/LeanCode.Logging/8.0.2260-preview/) | `BaseSanitizer`, `Placeholder` |

## Defining sanitizer

The following example demonstrates the creation of a custom sanitizer designed to redact an employee's password from logs when utilizing the `RegisterEmployee` command. Ensure that all sanitizers reside within the same assembly as the one provided to the `ConfigureDefaultLogging` method to properly work.

```csharp
public class RegisterEmployeeSanitizer : BaseSanitizer<RegisterEmployee>
{
    protected override RegisterEmployee TrySanitize(RegisterEmployee obj)
    {
        if (obj.Password != Placeholder)
        {
            return new RegisterEmployee
            {
                Name = obj.Name,
                Password = Placeholder,
            };
        }
        else
        {
            return null;
        }
    }
}
```

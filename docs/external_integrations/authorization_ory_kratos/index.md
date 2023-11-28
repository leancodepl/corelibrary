# Authorization - Ory Kratos

Ory Kratos is an identity and user management system built for cloud-based environments. It focuses on security while offering developer-friendly tools. This open-source project provides a flexible platform for developers to customize and integrate authentication workflows, ensuring a seamless user experience in the cloud. For more information, you can explore the official [Ory Kratos GitHub repository](https://github.com/ory/kratos).

## Why Ory Kratos?

The main thing that sets Kratos apart from all the other identity management solutions is that it is API-only, it is maintained according to current DevOps standards and has declarative configuration.

Ory Kratos has generic email+password flow (with optional verification), there are social logins, and WebAuthN. It also standardizes all the accompanying flows, like registration, email/password change, password reset, email verification, MFA, and alike. For a more in-depth exploration of why Kratos is our preferred choice, you can refer to this [article](https://leancode.co/blog/identity-management-solutions-part-2-the-choice).

## Packages

| Package | Link | Application in section |
| --- | ----------- | ----------- |
| LeanCode.Kratos | [![NuGet version (LeanCode.Kratos)](https://img.shields.io/nuget/vpre/LeanCode.Kratos.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/LeanCode.Kratos/8.0.2260-preview/) | Configuration |

## LeanCode CoreLibrary integration

LeanCode CoreLibrary provides 3 main components to integrate with Kratos:

1. [KratosAuthenticationHandler] - converts Kratos cookie into claims.
2. [KratosWebHookHandlerBase] - provides functionality to handle Kratos webhooks and helps with serialization and deserialization of messages from/to Kratos.
3. `IServiceCollection` extension methods - which allow to register [KratosAuthenticationHandler] and Kratos `IFrontendApi`, `IIdentityApi` API clients.

Ory Kratos can be either hosted on [Ory Network](https://www.ory.sh/network/) or be self-hosted.

> **Tip:** To see quickstart about how you can run self-hosted Kratos instance visit [here](https://www.ory.sh/docs/kratos/quickstart).

## Configuration

To integrate Kratos into LeanCode CoreLibrary-based application, you can follow the example below. This example demonstrates the use of the `AddKratos(...)` method to register the [KratosAuthenticationHandler]. It also adds the `user` role to every identity and assigns the `admin` role to every identity with an email in the `@leancode.pl` domain. Additionally, this example registers the `IFrontendApi` and `IIdentityApi` classes for interaction with the Kratos API:

```csharp
public override void ConfigureServices(IServiceCollection services)
{
    // . . .
    services
        .AddAuthentication()
        .AddKratos(options =>
        {
            options.NameClaimType = KnownClaims.UserId;
            options.RoleClaimType = KnownClaims.Role;

            options.ClaimsExtractor = (s, o, c) =>
            {
                // Every identity is a valid User
                c.Add(new(o.RoleClaimType, "user"));

                if (
                    s.Identity.VerifiableAddresses.Any(
                        kvia =>
                            kvia.Via == "email"
                            && kvia.Value.EndsWith(
                                "@leancode.pl",
                                false,
                                CultureInfo.InvariantCulture)
                            && kvia.Verified
                    )
                )
                {
                    c.Add(new(o.RoleClaimType, "admin"));
                }
            };
        });

    services.AddKratosClients(builder =>
    {
        // Kratos public endpoint
        builder.AddFrontendApiClient("");

        // Kratos admin endpoint
        builder.AddIdentityApiClient("");
    });

    // Api key which will be send by Kratos
    services.AddSingleton(new LeanCode.Kratos.KratosWebHookHandlerConfig(""));
    // . . .
}
```

[KratosAuthenticationHandler]: https://github.com/leancodepl/corelibrary/blob/v8.0-preview/src/Infrastructure/LeanCode.Kratos/KratosAuthenticationHandler.cs
[KratosWebHookHandlerBase]: https://github.com/leancodepl/corelibrary/blob/v8.0-preview/src/Infrastructure/LeanCode.Kratos/KratosWebHookHandlerBase.cs

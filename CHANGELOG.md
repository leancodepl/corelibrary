# Change Log
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/)
but this project DOES NOT adhere to [Semantic Versioning](http://semver.org/).

## 6.0

* Upgrade to .NET 6
* Remove `IdentityProvider`
* Remove `System.Time` (it is available in .NET 6)
* Promote using Azure MSI & add LeanCode Azure default credential provider
* Introduce `SId` & `LId`
* Switch to OpenTracing
* Remove LeanCode.ContractsGenerator in favor of the new [contracts generator](https://github.com/leancodepl/contractsgenerator)
* Package bump

## 5.1

* Remove `LeanCode.Facebook`,
* Support FB in `LeanCode.ExternalIdentityProviders`
* Change `ValueObject` to abstract record

## 5.0

* Upgrade to .NET 5.0
* Use `CancellationToken` in CQRS pipeline and some integrations
* Import `System.Time` from corefxlab as `LeanCode.Time`
* Add strongly–typed IDs (`Id<T>` / `IId<T>`)
* Remove `IUnitOfWork`, `LeanCode.PushNotifications` and in–proc event handlers
* Rework PdfRocket integration
* Throw exceptions on SendGrid call failures
* Bump packages (MassTransit to v7, IdentityServer4 to v4)

## 4.2
* MassTransit inbox/outbox pattern

## 4.1
* Upgrade to .NET Core 3.1

## 4.0
* Upgrade to .NET Core 3.0
* Bump C# language version to 8.0
* Add support for Nullable Reference Types
* Multiple breaking changes related to Razor, events, async methods and more
* Remove now redundant calls to ConfigureAwait in async methods
* Replace Newtonsoft.Json with System.Text.Json
* `AsyncTasks.Hangfire` does not prepare schema by default now
* `BackgroundProcessingApp` is now `HangfireTasksModule`
* Add MassTransit integration
* Replace hand-written SendGrid client with a small wrapper over official Client

## 3.5
* Rewrite of EFMigrator

## 3.4
### Changed
* All projects must be Localized now
* AutoMapper is no longer supported

## 3.3
### Changed
* Upgrade to .NET Core 2.2 & Newtonsoft.Json 12
* Switch most of the projects to netcoreapp2.2
* Embed source in nupkgs and do not publish symbol packages
* Switch to FluentValidation 8, which results in breaking change: `RuleForAsync` is now instance method of `ContextualValidator`
* Build is now done on Jenkins

## 3.2
Switch to dockerized build (and new FAKE)
Cleanup shared target files and remove LeanCode.Dependencies

## 3.1
Switch to .NET Core 2.1

## 3.0
The great versioning

## 0.3
Migrate to .NET Core 2
Migrate to Razor 2

## 0.2
Separated CQRS public interfaces/implementations
Pipelines
## 0.1
Initial release with base packages
Basic CQRS, domain models, basic infrastructure
## 0.0
Project initiation, no packages were released.

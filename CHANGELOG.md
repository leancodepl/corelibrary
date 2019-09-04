# Change Log
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/)
but this project DOES NOT adhere to [Semantic Versioning](http://semver.org/).

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

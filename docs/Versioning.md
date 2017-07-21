### Versioning

These packages have to be correctly versioned. Use [Semantic Versioning](http://semver.org), i.e. if you modify (change/remove) something from the public API (make breaking change), you have to bump major version, otherwise change in minor/patch version is enough. Also - write down all changes in the [changelog](CHANGELOG.md).

Change in required dependencies (major version bump) may be also considered a breaking change.

To simplify versioning process, there exists `Version.targets` file that specifies version. This forces all libs to have the same version. The file is generated automatically by `build.fsx`, based on `CHANGELOG.md`, so changes should be made there, not in the `Version.targets` file.

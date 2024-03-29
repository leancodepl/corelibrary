﻿namespace LeanCode.ForceUpdate;

public sealed record IOSVersionsConfiguration(Version MinimumRequiredVersion, Version CurrentlySupportedVersion);

public sealed record AndroidVersionsConfiguration(Version MinimumRequiredVersion, Version CurrentlySupportedVersion);

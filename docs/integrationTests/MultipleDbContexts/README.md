# ConfigurationOverrides class

ConfigurationOverrides class provides a way to override configuration values at runtime using a dictionary of key-value pairs.


## Methods

#### `CreateBuilder() => OverridesBuilder`

Creates a new instance of the `OverridesBuilder` class, which allows for fluent configuration of overrides.

#### `Build(IConfigurationBuilder builder) => IConfigurationProvider`

Builds a new `IConfigurationProvider` instance from the provided `IConfigurationBuilder`, using the current set of overrides.

#### `OverrideDatabaseConnectionString(string dbPrefix, string sourceEnv = ConnectionStringBaseDefault, string varName = "Database") => string`

Generates a new connection string based on a prefix and an environment variable, with a unique database name.

- `dbPrefix` (required): The prefix to use for the database name.
- `sourceEnv` (optional): The name of the environment variable containing the base connection string. Defaults to `SqlServer__ConnectionStringBase`.
- `varName` (optional): The name of the parameter to use for the database name in the connection string. Defaults to `"Database"`.

Returns a new connection string with a unique database name.

## OverridesBuilder class

Allows for fluent configuration of overrides using the `OverridesBuilder` class.

#### `AddValue(string key, string? value) => OverridesBuilder`

Adds a key-value pair to the overrides dictionary.

- `key` (required): The key to use for the configuration value.
- `value` (optional): The value to use for the configuration value. May be `null`.

Returns the `OverridesBuilder` instance, for fluent configuration.

#### `AddConnectionString(string key, string dbPrefix, string sourceEnv = ConnectionStringBaseDefault, string varName = "Database") => OverridesBuilder`

Adds a connection string to the dictionary, using the specified key and a unique database name generated from the specified prefix.

- `key` (required): The key to use for the connection string.
- `dbPrefix` (required): The prefix to use for the database name in the connection string.
- `sourceEnv` (optional): The name of the environment variable containing the base connection string. Defaults to `SqlServer__ConnectionStringBase`.
- `varName` (optional): The name of the parameter to use for the database name in the connection string. Defaults to `"Database"`.

Returns the `OverridesBuilder` instance, for fluent configuration.

#### `Build() => ConfigurationOverrides`

Builds a new `ConfigurationOverrides` instance using the current set of overrides.

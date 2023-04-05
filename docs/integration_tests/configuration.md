# ConfigurationOverrides

`ConfigurationOverrides` is a helper class that provides a way to override configuration values at runtime using a dictionary of key-value pairs. This can be particularly useful during integration testing when you need to modify specific configuration values without affecting the original configuration values. What is more `ConfigurationOverrides` allows multiple connections strings.

## Usage with a Single DbContext

If you have a single DbContext in your application, you can use `ConfigurationOverrides` to override the connection string for that DbContext during testing. Here's an example:

```csharp
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var overrides = new ConfigurationOverrides()
    .AddConnectionString(ConfigurationOverrides.ConnectionStringKeyDefault, "MyDatabase");

var connectionString = configuration.GetConnectionString(ConfigurationOverrides.ConnectionStringKeyDefault);

var dbContextOptions = new DbContextOptionsBuilder<MyDbContext>()
    .UseSqlServer(connectionString)
    .Options;

var dbContext = new MyDbContext(dbContextOptions);
```
In this example, we first create a new ConfigurationBuilder and load the application's original configuration values from the appsettings.json file. We then create a new instance of ConfigurationOverrides and use the AddConnectionString method to add a new connection string to the dictionary. This will override the original connection string for the MyDbContext.

Next, we retrieve this connection string using the GetConnectionString method of the Configuration object. Finally, we create a new DbContextOptionsBuilder using the connection string, and pass that to the constructor of our DbContext.

## Usage with Multiple DbContexts

If you have multiple DbContexts in your application, you can use `ConfigurationOverrides` to override the connection strings for each DbContext during testing. Here's an example:

```csharp
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var overrides = new ConfigurationOverrides()
    .AddConnectionString("SqlServer__ConnectionStringBase", "MyDatabase1")
    .AddConnectionString("SqlServer__ConnectionStringSecondary", "MyDatabase2");

var connectionStringBase = configuration.GetConnectionString("SqlServer__ConnectionStringBase");
var connectionStringSecondary = configuration.GetConnectionString("SqlServer__ConnectionStringSecondary");

var dbContextOptionsBase = new DbContextOptionsBuilder<MyDbContextBase>()
    .UseSqlServer(connectionStringBase)
    .Options;

var dbContextOptionsSecondary = new DbContextOptionsBuilder<MyDbContextSecondary>()
    .UseSqlServer(connectionStringSecondary)
    .Options;

var dbContextBase = new MyDbContextBase(dbContextOptionsBase);
var dbContextSecondary = new MyDbContextSecondary(dbContextOptionsSecondary);
```

In this example, we first create a new ConfigurationBuilder and load the application's original configuration values from the appsettings.json file. We then create a new instance of ConfigurationOverrides and use the AddConnectionString method to add two new connection strings to the dictionary.

Next, we retrieve connection strings using the GetConnectionString method of the Configuration object. Finally, we create new DbContextOptionsBuilder objects using the original connection strings, and pass those to the constructors of our DbContexts.



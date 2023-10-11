# LeanCode.AuditLogs

Package dedicated to auditing changes of entities (including domain objects) modified during execution of command/operation/event handlers.

Package uses EntityFramework's ChangeTracker in order to detect changes. Each log consists of:
- object type
- object identifier
- handler name
- date of the change
- actor executing changes
- object changes

## Dependencies

`LeanCode.AuditLogs` depend on `IdentityTraceAttributesMiddleware` from  `LeanCode.OpenTelemetry` - if this middleware is not configured, then the `ActorId` will be always set to `null`.

## Configuration

The package does not require extra work from the user other than initial configuration. In order to collect audit logs from all handlers there are three things to configure.

## AuditLogsConsumer

The audit logs are collected and processed asynchronously by dedicated consumer. Usually event handlers are registered within the project, so adding the `AuditLogsConsumer` to the list of consumers should suffice to configure everything correctly.

```csharp

    public override void ConfigureServices(IServiceCollection services)
    {
        // some other code

            cfg.AddConsumersWithDefaultConfiguration(
                AllHandlers.Assemblies.ToArray().Append(typeof(AuditLogsConsumer).Assembly),
                typeof(DefaultConsumerDefinition<>)
            );

        // some other code
```

⚠️ Remember that if you configure the audit log in the same way as other consumers it's audit log will also be recorded. In order to avoid recursive logging of changes do not use the same `DbContext` for audit logs as you use for the business part of the system

### AuditLogStorage

AuditLogs collectors require `IAuditLogStorage` to be registered in the DI container.

```csharp
public override void ConfigureServices(IServiceCollection services)
{
    // some other code

    services.AddTransient<IAuditLogStorage, YourAuditLogStorage>();

    // some other code
}
```

Currently there are couple of ready made implementations of `IAuditLogStorage`:

#### StubAuditLogStorage

Sample implementation that logs audit information using `Serilog` logger.

#### AzureBlobAuditLogStorage

Storage implementation using Azure Blob Storage service. It stores log for each entity as a separate append blob.

This storage assumes that there is azure storage client configured in DI. It also requires the container name passed via `AzureBlobAuditLogStorageConfiguration` to know where to store audit log files. The storage assumes that container specified as `AuditLogsContainer` is already created (private access is mandatory) and `AuditLogsTable` is already created.

Example configuration looks like this:

```csharp
public override void ConfigureServices(IServiceCollection services)
{
    // some other code

    services.AddAzureClients(cfg =>
    {
        cfg.AddBlobServiceClient(Config.BlobStorage.ConnectionString(config));
        cfg.AddTableServiceClient(Config.BlobStorage.ConnectionString(config));
    });
    services.AddSingleton(new AzureBlobAuditLogStorageConfiguration("audit-logs", "auditlogs"));
    services.AddTransient<IAuditLogStorage, AzureBlobAuditLogStorage>();

    // some other code
}
```

#### Other options

If you want to use some other store for your data feel free to implement `IAuditLogStorage` on your own.

### Endpoints

In order to collect audit logs you need to add `Audit<TDbContext>()` middleware to execution pipeline. The `TDbContext` argument is a `DbContext` where we want to audit the changes.

Example configuration using AuditLogs looks as follows:

```csharp
protected override void ConfigureApp(IApplicationBuilder app)
{
    // some other code

    app.UseEndpoints(
        endpoints => endpoints.MapRemoteCqrs(
            "/api",
            cqrs =>
            {
                cqrs.Commands = c =>
                    c.CQRSTrace()
                        .Secure()
                        .Validate()
                        .CommitTransaction<CoreDbContext>()
                        .PublishEvents()
                        .Audit<CoreDbContext>();
                cqrs.Queries = c => c.CQRSTrace().Secure();
                cqrs.Operations = c =>
                    c.CQRSTrace()
                        .Secure()
                        .CommitTransaction<CoreDbContext>()
                        .PublishEvents()
                        .Audit<CoreDbContext>();
            }
        )
    );

    // some other code
}
```

⚠️ Bear in mind, that the order here makes difference. If you don't want to collect changes in the MT inbox/outbox tables, then you should configure `Audit<TDbContext>()` middleware **after** the `PublishEvents()` middleware.

### Consumers

In order to add audit logs to event handling the only thing you need to do is to add `.UseAuditLogs<TDbContext>(sp)` to the consumer configuration.

```csharp
protected override void ConfigureConsumer(
    IReceiveEndpointConfigurator endpointConfigurator,
    IConsumerConfigurator<TConsumer> consumerConfigurator,
    IRegistrationContext context
)
{
    endpointConfigurator.UseRetry(
        r => r.Immediate(1).Incremental(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5))
    );
    endpointConfigurator.UseEntityFrameworkOutbox<CoreDbContext>(context);
    endpointConfigurator.UseDomainEventsPublishing(context);
    endpointConfigurator.UseAuditLogs<CoreDbContext>(context);
}
```

⚠️ Bear in mind, that the order here makes difference. If you don't want to collect changes in the MT inbox/outbox tables, then you should configure `UseAuditLogs<TDbContext>(sp)` filter **after** the `UseDomainEventsPublishing(sp)` filter.

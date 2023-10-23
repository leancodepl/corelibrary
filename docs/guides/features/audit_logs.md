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

The package does not require extra work from the user other than initial configuration. In order to collect audit logs from all handlers there are 3 things to configure.

### 1. AddAzureStorageAuditLogs

If you want to use audit logs stored on Azure Storage you need to add necessary configuration used by the audit logs package.

Storage implementation using Azure Blob Storage service stores log for each entity as a separate append blob. This storage assumes that there is azure storage client configured in DI. It also requires the container name passed via `AzureBlobAuditLogStorageConfiguration` to know where to store audit log files. The storage assumes that container specified as `AuditLogsContainer` is already created (private access is mandatory) and `AuditLogsTable` is already created.

```csharp
    public override void ConfigureServices(IServiceCollection services)
    {
        // some other code

        services.AddAzureClients(cfg =>
        {
            cfg.AddBlobServiceClient(Config.BlobStorage.ConnectionString(config));
            cfg.AddTableServiceClient(Config.BlobStorage.ConnectionString(config));
        });

        services.AddAzureStorageAuditLogs(new AzureBlobAuditLogStorageConfiguration("audit-logs", "auditlogs"));

        // some other code
    }
```

### 2. AddAuditLogsConsumer

The audit logs are collected and processed asynchronously by dedicated consumer. You need to add it to your MassTransit configuration

```csharp

    public override void ConfigureServices(IServiceCollection services)
    {
        // some other code

        services.AddCQRSMassTransitIntegration(cfg =>
        {
            // some other code

            cfg.AddAuditLogsConsumer();

            // some other code
        }

        // some other code
```

### 3. Endpoints

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

### 4. Consumers

In order to add audit logs to event handling the only thing you need to do is to add `.UseAuditLogs<TDbContext>(context)` to the consumer configuration.

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

⚠️ Bear in mind, that the order here makes difference. If you don't want to collect changes in the MT inbox/outbox tables, then you should configure `UseAuditLogs<TDbContext>(context)` filter **after** the `UseDomainEventsPublishing(context)` filter.

## Other options

If you want to use some other store for your data feel free to implement `IAuditLogStorage` on your own. Remember to register `AuditLogsPublisher` when configuring DI.

## Azure Storage infrastructure configuration

When configuring cloud resources in cloud you should remember about the right to be forgotten and costs of storing the data based on the access tiers. Below you can find a terraform snippet that configures automatic tier changes and deletion based on the last blob modification time. ⚠️ Bear in mind that the values provided below may not be valid for your use case.

```terraform
resource "azurerm_storage_management_policy" "decrease_access_tier" {
  storage_account_id = azurerm_storage_account.audit_logs_storage.id

  rule {
    name    = "archive_and_delete_old_log_entries"
    enabled = true
    filters {
      prefix_match = ["{$YOUR_AUDIT_LOGS_CONTAINER_NAME}/"]
      blob_types   = ["appendBlob"]
    }
    actions {
      base_blob {
        tier_to_cool_after_days_since_modification_greater_than    = 28
        auto_tier_to_hot_from_cool_enabled                         = true
        tier_to_archive_after_days_since_modification_greater_than = 84
        delete_after_days_since_modification_greater_than          = 365
      }
    }
  }
}
```

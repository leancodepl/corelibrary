using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Serilog;

namespace LeanCode.AuditLogs;

public class AzureBlobAuditLogStorage : IAuditLogStorage
{
    private readonly ILogger logger = Log.ForContext<AzureBlobAuditLogStorage>();

    private static ReadOnlySpan<byte> NewLineBytes => "\n"u8;
    private static readonly JsonSerializerOptions Options =
        new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = false,
        };

    private readonly BlobServiceClient blobClient;
    private readonly TableServiceClient tableClient;
    private readonly AzureBlobAuditLogStorageConfiguration config;

    public AzureBlobAuditLogStorage(
        BlobServiceClient blobClient,
        TableServiceClient tableClient,
        AzureBlobAuditLogStorageConfiguration config
    )
    {
        this.blobClient = blobClient;
        this.tableClient = tableClient;
        this.config = config;
    }

    public async Task StoreEventAsync(AuditLogMessage auditLogMessage, CancellationToken cancellationToken)
    {
        try
        {
            await AppendLogToBlobAsync(auditLogMessage, cancellationToken);
        }
        catch (RequestFailedException e) when (e.ErrorCode == BlobErrorCode.BlockCountExceedsLimit)
        {
            await BumpSuffixInTableAsync(auditLogMessage, cancellationToken);
            await AppendLogToBlobAsync(auditLogMessage, cancellationToken);
        }
    }

    private async Task AppendLogToBlobAsync(AuditLogMessage auditLogMessage, CancellationToken cancellationToken)
    {
        var blob = await CreateBlobAsync(auditLogMessage, cancellationToken);
        using var stream = Serialize(auditLogMessage);
        await blob.AppendBlockAsync(stream, cancellationToken: cancellationToken);

        logger.Verbose("Log append to the blob {BlobName}", blob.Name);
    }

    private async Task<AppendBlobClient> CreateBlobAsync(
        AuditLogMessage auditLogMessage,
        CancellationToken cancellationToken
    )
    {
        var container = blobClient.GetBlobContainerClient(config.AuditLogsContainer);
        var table = tableClient.GetTableClient(config.AuditLogsTable);
        var suffix = await GetSuffixAsync(auditLogMessage, table, cancellationToken);

        var blobName = GetBlobName(auditLogMessage.EntityChanged, suffix);

        var blob = container.GetAppendBlobClient(blobName);
        await blob.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        return blob;
    }

    private static MemoryStream Serialize(AuditLogMessage auditLogMessage)
    {
        var stream = new MemoryStream();
        JsonSerializer.Serialize(stream, auditLogMessage, Options);
        stream.Write(NewLineBytes);
        stream.Position = 0;
        return stream;
    }

    private static async Task<int> GetSuffixAsync(
        AuditLogMessage auditLogMessage,
        TableClient table,
        CancellationToken cancellationToken
    )
    {
        var res = await table.GetEntityIfExistsAsync<TableEntity>(
            auditLogMessage.EntityChanged.Type,
            string.Join("", auditLogMessage.EntityChanged.Ids),
            cancellationToken: cancellationToken
        );
        if (!res.HasValue)
        {
            var entity = new TableEntity(
                auditLogMessage.EntityChanged.Type,
                string.Join("", auditLogMessage.EntityChanged.Ids)
            )
            {
                ["Suffix"] = 0,
            };
            await table.AddEntityAsync(entity, cancellationToken: cancellationToken);

            return (int)entity["Suffix"];
        }
        else
        {
            return (int)res.Value["Suffix"];
        }
    }

    private async Task BumpSuffixInTableAsync(AuditLogMessage auditLogMessage, CancellationToken cancellationToken)
    {
        var table = tableClient.GetTableClient(config.AuditLogsTable);
        var res = await table.GetEntityAsync<TableEntity>(
            auditLogMessage.EntityChanged.Type,
            string.Join("", auditLogMessage.EntityChanged.Ids),
            cancellationToken: cancellationToken
        );
        var entity = res.Value;
        entity["Suffix"] = (int)entity["Suffix"] + 1;
        await table.UpdateEntityAsync(entity, entity.ETag, cancellationToken: cancellationToken);
    }

    private static string GetBlobName(EntityData entity, int suffix)
    {
        return $"{entity.Type}/{string.Join("", entity.Ids)}.{suffix}";
    }
}

public record AzureBlobAuditLogStorageConfiguration(string AuditLogsContainer, string AuditLogsTable);

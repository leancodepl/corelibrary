using System.Text;
using System.Text.Json;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FluentAssertions;
using LeanCode.AuditLogs;
using LeanCode.TimeProvider;
using Xunit;

namespace LeanCode.Azure.Tests;

public class AzureStorageAuditLogIntegrationTests
{
    private readonly BlobServiceClient blobServiceClient;
    private readonly AzureBlobAuditLogStorage storage;

    public AzureStorageAuditLogIntegrationTests()
    {
        var credential = Env.GetTokenCredential();
        blobServiceClient = new BlobServiceClient(
            new Uri(Environment.GetEnvironmentVariable(Env.AzureBlobStorageServiceUriKey)!),
            credential
        );
        storage = new AzureBlobAuditLogStorage(
            blobServiceClient,
            new TableServiceClient(
                new Uri(Environment.GetEnvironmentVariable(Env.AzureTableStorageServiceUriKey)!),
                credential
            ),
            new(
                Environment.GetEnvironmentVariable(Env.AzureBlobStorageContainerNameKey)!,
                Environment.GetEnvironmentVariable(Env.AzureTableStorageTableNameKey)!
            )
        );
    }

    [AzureStorageFact]
    public async Task Ensure_that_log_is_correctly_uploaded_to_storage()
    {
        var type = "type";
        var id = Guid.NewGuid();
        await storage.StoreEventAsync(
            new AuditLogMessage(
                new(
                    [id.ToString()],
                    type,
                    JsonSerializer.SerializeToDocument(new { Foo = "bar" }),
                    JsonSerializer.SerializeToDocument(new { Shadow = "property" }),
                    "state"
                ),
                "actionName",
                Time.NowWithOffset,
                "actorId",
                "traceId",
                "spanId"
            ),
            default
        );

        var (lineCount, blobCount) =  await CheckLinesAndBlobsCountAsync(type, id);

        lineCount.Should().Be(1);
        blobCount.Should().Be(1);
    }



    [AzureStorageFact]
    public async Task Ensure_that_multiple_logs_are_correctly_uploaded_to_storage()
    {
        var logsToRecord = 12;
        var type = "type";
        var id = Guid.NewGuid();
        for (var i = 0; i < logsToRecord; i++)
        {
            await storage.StoreEventAsync(
                new AuditLogMessage(
                    new(
                        [id.ToString()],
                        type,
                        JsonSerializer.SerializeToDocument(new { Foo = "bar" }),
                        JsonSerializer.SerializeToDocument(new { Shadow = "property" }),
                        "state"
                    ),
                    "actionName",
                    Time.NowWithOffset,
                    "actorId",
                    "traceId",
                    "spanId"
                ),
                default
            );
        }

        var containerClient = blobServiceClient.GetBlobContainerClient(
            Environment.GetEnvironmentVariable(Env.AzureBlobStorageContainerNameKey)
        );

        var (lineCount, blobCount) =  await CheckLinesAndBlobsCountAsync(type, id);

        blobCount.Should().Be(1);
        lineCount.Should().Be(logsToRecord);
    }

    [AzureStorageFact(Skip = "This test is too long")]
    public async Task Ensure_that_all_logs_are_correctly_uploaded_to_multiple_files()
    {
        var logsToRecord = 51000;
        var type = "type";
        var id = Guid.NewGuid();
        for (var i = 0; i < logsToRecord; i++)
        {
            await storage.StoreEventAsync(
                new AuditLogMessage(
                    new(
                        [id.ToString()],
                        type,
                        JsonSerializer.SerializeToDocument(new { Foo = "bar" }),
                        JsonSerializer.SerializeToDocument(new { Shadow = "property" }),
                        "state"
                    ),
                    "actionName",
                    Time.NowWithOffset,
                    "actorId",
                    "traceId",
                    "spanId"
                ),
                default
            );
        }

        var containerClient = blobServiceClient.GetBlobContainerClient(
            Environment.GetEnvironmentVariable(Env.AzureBlobStorageContainerNameKey)
        );

        var (lineCount, blobCount) =  await CheckLinesAndBlobsCountAsync(type, id);

        blobCount.Should().NotBe(1);
        lineCount.Should().Be(logsToRecord);
    }
        private async Task<(int LineCount, int BlobCount)> CheckLinesAndBlobsCountAsync(string type, Guid id)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(
            Environment.GetEnvironmentVariable(Env.AzureBlobStorageContainerNameKey)
        );

        var blobsFound = 0;
        var linesFound = 0;
        await foreach (var b in containerClient.GetBlobsAsync(BlobTraits.All, BlobStates.All, $"{type}/{id}"))
        {
            var blockBlob = containerClient.GetBlobClient(b.Name);
            using var stream = new MemoryStream();
            await blockBlob.DownloadToAsync(stream);

            stream.Position = 0;

            linesFound += stream.ToArray().Count(c => c == '\n');
            blobsFound++;
        }

        return (linesFound, blobsFound);
    }
}

public sealed class AzureStorageFactAttribute : FactAttribute
{
    public AzureStorageFactAttribute()
    {
        Skip ??= Env.SkipIfVariablesNotSet(
            Env.TenantIdKey,
            Env.ClientIdKey,
            Env.ClientSecretKey,
            Env.AzureBlobStorageServiceUriKey,
            Env.AzureTableStorageServiceUriKey
        );
    }
}

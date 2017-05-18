using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace LeanCode.DataStorage
{
    class AzureDataStorage : IDataStorage
    {
        private readonly CloudStorageAccount storageAccount;
        private const string containerName = "datastorage";

        public AzureDataStorage(IOptions<AzureStorageConfiguration> storageConfiguration)
        {
            var config = storageConfiguration.Value;

            storageAccount = CloudStorageAccount.Parse(config.ConnectionString);
        }

        public string GeneratePathWithRandomFileName(string directory, string extension)
        {
            directory = directory.Trim('/', '\\');
            extension = extension.StartsWith(".") ? extension : "." + extension;

            return $"{directory}/{Guid.NewGuid()}{extension}";
        }

        public async Task<string> UploadFile(string path, Stream data)
        {
            var container = await GetContainer();

            var blob = container.GetBlockBlobReference(path);
            await blob.UploadFromStreamAsync(data);

            return blob.Uri.ToString();
        }

        private async Task<CloudBlobContainer> GetContainer()
        {
            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(containerName);
            await container.CreateIfNotExistsAsync();
            await container.SetPermissionsAsync(new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            });
            return container;
        }
    }
}

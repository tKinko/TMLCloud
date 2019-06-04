using System;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureStorageExtensions
{
    public class AzureStorageConfig
    {
        public string ConnectionString { get; set; }
        public string Container { get; set; }
    }
    public class StorageFileConfig
    {
        public string FilePath { get; set; }
        public Stream Stream { get; set; }
        public string StrageFilename { get; set; }
    }
    public class AzureStorageExtension
    {
        static private CloudBlobContainer GetBlobContainer(AzureStorageConfig _storageConfig)
        {
            // Create cloudstorage account by passing the storageconnectionstring
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_storageConfig.ConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Get reference to the blob container by passing the name by reading the value from the configuration (appsettings.json)
            CloudBlobContainer container = blobClient.GetContainerReference(_storageConfig.Container);

            container.CreateIfNotExistsAsync();

            return container;
        }
        static public string UploadLog(AzureStorageConfig _storageConfig, string content)
        {
            string log = "";

            CloudBlobContainer container = GetBlobContainer(_storageConfig);
            CloudBlockBlob blob = container.GetBlockBlobReference(string.Format("{0}_log.txt", _storageConfig.Container));
            try
            {
                blob.UploadTextAsync(content).Wait();
            }
            catch { };
            return log;
        }
        public string DownloadLog(AzureStorageConfig _storageConfig)
        {
            string log = "";

            CloudBlobContainer container = GetBlobContainer(_storageConfig);
            CloudBlockBlob blob = container.GetBlockBlobReference(string.Format("{0}_log.txt", _storageConfig.Container));
            try
            {
                log = blob.DownloadTextAsync().Result;
            }
            catch { };
            return log;
        }
    }
}

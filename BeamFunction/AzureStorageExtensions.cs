using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace TDS7130v2AC.Extensions
{
    public class AzureStorageConfig
    {
        public string ConnectionString { get; set; }
        public string Container { get; set; }
    }
    public class UploadFileConfig
    {
        public string UploadFilePath { get; set; }
        public Stream Stream { get; set; }
        public string StrageFilename { get; set; }
    }
    public class AzureStorageExtensions
    {
        public AzureStorageExtensions()
        { }

        private static CloudBlobContainer GetBlobContainer(AzureStorageConfig _storageConfig)
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


        public string GetGraphsLog(AzureStorageConfig _storageConfig)
        {
            return GetLogText(_storageConfig);
        }
        //public string GetInfomationLog()
        //{
        //    return GetLogText("Infomation");
        //}
        public string GetLogText(AzureStorageConfig _storageConfig)
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
       public Task<List<CloudBlob>> GetGraphsUrlAsync(AzureStorageConfig _storageConfig)
        {
            return Task<List<CloudBlob>>.Run(() => {
                CloudBlobContainer container = GetBlobContainer(_storageConfig);
                List<CloudBlob> blobs = new List<CloudBlob>();
                try
                {
                    BlobResultSegment resultSegment = container.ListBlobsSegmentedAsync(null).Result;

                    foreach (IListBlobItem item in resultSegment.Results)
                    {
                        if (item.GetType() == typeof(CloudBlockBlob))
                        {
                            CloudBlockBlob blob = (CloudBlockBlob)item;
                            if (blob.Properties.ContentType.Contains("text"))
                                continue;

                            blobs.Add(blob);
                            //blobs.Add(blob.StorageUri.PrimaryUri.AbsoluteUri);
                        }
                        else if (item.GetType() == typeof(CloudPageBlob))
                        {
                            CloudPageBlob blob = (CloudPageBlob)item;
                            blobs.Add(blob);
                            //blobs.Add(blob.StorageUri.PrimaryUri.AbsoluteUri);
                        }
                        // else if (item.GetType() == typeof(CloudBlobDirectory))
                        // {
                        //    CloudBlobDirectory dir = (CloudBlobDirectory)item;
                        //    blobs.Add(dir.Uri.ToString());
                        //}
                    }
                }
                catch { }
                return blobs;
            });
        }
        //public Task<TdsDataObject> GetTable(AzureStorageConfig _storageConfig)
        //{
        //    return Task<TdsDataObject>.Run(() => {
        //        TdsDataObject tableObject = new TdsDataObject();
        //        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_storageConfig.ConnectionString);

        //        CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

        //        tableObject.InitTables(_storageConfig.Container, tableClient);

        //        return tableObject;
        //    });
        //}
        static public string SaveInfoLog(AzureStorageConfig _storageConfig,string content)
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
        public string GetInfoLog(AzureStorageConfig _storageConfig)
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
        public Task<List<string>> GetInfotext(AzureStorageConfig _storageConfig)
        {
            return Task<List<string>>.Run(async () => {
                List<string> blobs = null;
                CloudBlobContainer container = GetBlobContainer(_storageConfig);
                CloudBlockBlob blob = container.GetBlockBlobReference("_Info.txt");

                try
                {
                    string text = await blob.DownloadTextAsync(Encoding.UTF8,null,null,null);
                    blobs = new List<string>(text.Split(new string[] { "\r\n" }, StringSplitOptions.None));
                }
                catch { };

                return blobs;
            });
        }
        public static async Task<bool> UploadStream(UploadFileConfig _fileConfig, AzureStorageConfig _storageConfig)
        {
            // Get reference to the blob container by passing the name by reading the value from the configuration (appsettings.json)
            CloudBlobContainer container = GetBlobContainer(_storageConfig);

            CloudBlockBlob blockBlob = container.GetBlockBlobReference(_fileConfig.StrageFilename);

            await blockBlob.UploadFromStreamAsync(_fileConfig.Stream);

            return await Task.FromResult(true);
        }
        public static async Task<bool> DownloadStream(UploadFileConfig _fileConfig, AzureStorageConfig _storageConfig)
        {
            // Get reference to the blob container by passing the name by reading the value from the configuration (appsettings.json)
            CloudBlobContainer container = GetBlobContainer(_storageConfig);

            CloudBlockBlob blockBlob = container.GetBlockBlobReference(_fileConfig.StrageFilename);

            if(blockBlob.ExistsAsync().Result == false)
                return await Task.FromResult(false);

            await blockBlob.DownloadToStreamAsync(_fileConfig.Stream);

            return await Task.FromResult(true);
        }
    }
}

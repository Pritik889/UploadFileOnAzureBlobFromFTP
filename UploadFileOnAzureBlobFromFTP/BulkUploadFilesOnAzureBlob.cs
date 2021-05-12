using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Renci.SshNet;
using System.IO;
using System.Threading.Tasks;

namespace UploadFileOnAzureBlobFromFTP
{
    public class BulkUploadFilesOnAzureBlob : IBulkUploadFilesOnAzureBlob
    {
        private readonly FTPConfigurations _ftPConfigurations;
        private readonly BlobStorageConfigurations _blobStorageConfigurations;
        private readonly SftpClient _sftpClient;
        /// <summary>
        /// call constructor to initialize the configuration
        /// </summary>
        /// <param name="fTPConfigurations">ftp configuration required</param>
        /// <param name="blobStorageConfigurations">Blob configuration required</param>
        public BulkUploadFilesOnAzureBlob(FTPConfigurations fTPConfigurations, BlobStorageConfigurations blobStorageConfigurations)
        {
            _ftPConfigurations = fTPConfigurations;
            _blobStorageConfigurations = blobStorageConfigurations;
            if (_sftpClient == null)
            {
                _sftpClient = new SftpClient(_ftPConfigurations.IPAddress, _ftPConfigurations.UserName, _ftPConfigurations.Password);
            }

            if (!_sftpClient.IsConnected)
            {
                _sftpClient.Connect();
            }
        }


        public async Task<int> UploadFilesToAzureBlobAsync()
        {
            try
            {
                var sftpFile = _sftpClient.ListDirectory(_ftPConfigurations.FtpPathFolder);
                var iCountFile = 0;
                foreach (var item in sftpFile)
                {
                    if (item.IsRegularFile)
                    {
                        // Retrieve storage account from the connection string.
                        var storageAccount = CloudStorageAccount.Parse(_blobStorageConfigurations.ConnectionString);
                        // Create the blob client.
                        var blobClient = storageAccount.CreateCloudBlobClient();
                        // Retrieve a reference to a previously created container.
                        CloudBlobContainer container = blobClient.GetContainerReference(_blobStorageConfigurations.ContainerName);
                        //string filename1 = "SampleData-11.csv";
                        //get Blob reference
                        CloudBlockBlob blockBlobReference = container.GetBlockBlobReference(item.Name);
                        //cloudBlockBlob.Properties.ContentType =.ContentType;
                        var filestream = DownloadFile(_sftpClient, item.FullName);
                        //upload file to blob
                        await blockBlobReference.UploadFromStreamAsync(filestream).ConfigureAwait(false);
                        iCountFile++;
                    }

                }
                return iCountFile;

            }
            finally
            {
                _sftpClient.Disconnect();
            }

        }

        private Stream DownloadFile(SftpClient _SftpClient, string sourcePath)
        {
            var memoryStream = new MemoryStream();

            _SftpClient.DownloadFile(sourcePath, memoryStream);

            memoryStream.Position = 0;

            return memoryStream;
        }

    }
}

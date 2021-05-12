using System.Threading.Tasks;

namespace UploadFileOnAzureBlobFromFTP
{
   public interface IBulkUploadFilesOnAzureBlob
    {
        Task<int> UploadFilesToAzureBlobAsync();
    }
}

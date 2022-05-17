namespace SmartStore.BlobStorageService
{
    public interface IBlobStorageService
    {
        Task<bool> DeleteBlobIfExistAsync(string url);
        Task<string> UploadBlobAsync(Stream file, string contentType, string fileName, string? container);
    }
}

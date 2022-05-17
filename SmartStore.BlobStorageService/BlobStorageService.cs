

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SmartStore.BlobStorageService.Common;
using System.Text.RegularExpressions;

namespace SmartStore.BlobStorageService
{
    public class BlobStorageService : IBlobStorageService
    {

        public async Task<bool> DeleteBlobIfExistAsync(string url)
        {
            if (string.IsNullOrEmpty(url)) return true;
            var isImg = CommonContains.AllowedImgExt.Any(y => url.EndsWith(y));

            var imageSizes = CommonContains.ImageSize();
            BlobContainerClient blobContainer = new BlobContainerClient("DefaultEndpointsProtocol=https;AccountName=voviecstorage;AccountKey=6BWk9HK3Due9UE836/Q92xBWzOQjAais9ufHOGozQvwGP1if3/eYbuat77DyCMwzw2ZPZpiVK5ecw9pe5Lx5fA==;EndpointSuffix=core.windows.net", (isImg ? "upload-images" : "upload-files"));
            var blobClient = blobContainer.GetBlobClient(url);

            if (blobContainer != null)
            {

                foreach (var imgSize in imageSizes)
                {
                    var newFileName = Regex.Replace(url, @"\.([^.]*)$", $"_{imgSize.Key}.jpeg");
                    var blobClientResize = blobContainer.GetBlobClient(newFileName);
                    _ = await blobClientResize.DeleteIfExistsAsync();
                }
                return await blobClient.DeleteIfExistsAsync();
            }
            return false;
        }

        public async Task<string> UploadBlobAsync(Stream file, string contentType, string fileName, string? container)
        {

            if (string.IsNullOrEmpty(contentType)) return "";
            var isImg = CommonContains.AllowedImgExt.Any(y => contentType.EndsWith(y));
            if (!isImg && !CommonContains.AllowedFileExt.Any(y => contentType.EndsWith(y))) return "";
            var _container = container ?? (isImg ? "upload-images" : "upload-files");
            BlobContainerClient blobContainer = new BlobContainerClient("DefaultEndpointsProtocol=https;AccountName=voviecstorage;AccountKey=6BWk9HK3Due9UE836/Q92xBWzOQjAais9ufHOGozQvwGP1if3/eYbuat77DyCMwzw2ZPZpiVK5ecw9pe5Lx5fA==;EndpointSuffix=core.windows.net", _container);

            var blobClient = blobContainer.GetBlobClient(fileName);
            BlobHttpHeaders httpHeaders = new BlobHttpHeaders()
            {
                ContentType = contentType
            };

            await blobClient.UploadAsync(file, httpHeaders);
            if (container == "upload-images")
                await ResizingImageUpload(file, fileName, _container);
            await blobContainer.ExistsAsync();
            return blobContainer.Uri.AbsoluteUri + "/" + fileName;
        }
        public async Task ResizingImageUpload(Stream file, string fileName, string container)
        {
            BlobContainerClient blobContainer = new BlobContainerClient("DefaultEndpointsProtocol=https;AccountName=voviecstorage;AccountKey=6BWk9HK3Due9UE836/Q92xBWzOQjAais9ufHOGozQvwGP1if3/eYbuat77DyCMwzw2ZPZpiVK5ecw9pe5Lx5fA==;EndpointSuffix=core.windows.net", container);

            var imageSizes = CommonContains.ImageSize();

            foreach (var imgSize in imageSizes)
            {
                var newFileName = Regex.Replace(fileName, @"\.([^.]*)$", $"_{imgSize.Key}.jpeg");
                var blobClient = blobContainer.GetBlobClient(newFileName);
                using (var outStream = new MemoryStream())
                {
                    using (var image = Image.Load(file))
                    {
                        var newSize = ResizingImage(image, imgSize.Width, imgSize.Height);
                        image.Mutate(x => x.Resize(newSize.width, newSize.heigth));
                        await image.SaveAsync(outStream, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder());
                    }
                    BlobUploadOptions blobUploadOptions = new BlobUploadOptions()
                    {
                        HttpHeaders = new BlobHttpHeaders()
                        {
                            ContentType = "image/jpeg"
                        }
                    };
                    await blobClient.UploadAsync(new BinaryData(outStream.ToArray()), blobUploadOptions);
                }
            }
            await blobContainer.ExistsAsync();
        }

        public (int width, int heigth) ResizingImage(Image image, int maxWidth, int maxHeight)
        {
            if (image.Width > maxWidth || image.Height > maxHeight)
            {
                double widthRadio = (double)image.Width / (double)maxWidth;
                double heightRadio = (double)image.Height / (double)maxHeight;
                double radio = Math.Max(widthRadio, heightRadio);

                int newWidth = (int)(image.Width / radio);
                int newHeight = (int)(image.Height / radio);
                return (newWidth, newHeight);
            }
            else
            {
                return (image.Width, image.Height);
            }
        }
    }
}

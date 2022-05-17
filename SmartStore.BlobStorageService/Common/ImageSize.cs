namespace SmartStore.BlobStorageService.Common
{
    public class ImageSize
    {
        public string Key { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public ImageSize(string key, int width, int height)
        {
            Key = key;
            Width = width;
            Height = height;
        }
    }
}
